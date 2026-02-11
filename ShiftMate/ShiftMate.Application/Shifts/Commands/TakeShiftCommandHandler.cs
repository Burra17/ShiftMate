using MediatR;
using ShiftMate.Application.Interfaces;
using Microsoft.EntityFrameworkCore;
using ShiftMate.Domain;

namespace ShiftMate.Application.Shifts.Commands
{
    public class TakeShiftCommandHandler : IRequestHandler<TakeShiftCommand, bool>
    {
        private readonly IAppDbContext _context;

        public TakeShiftCommandHandler(IAppDbContext context)
        {
            _context = context;
        }

        public async Task<bool> Handle(TakeShiftCommand request, CancellationToken cancellationToken)
        {
            // 1. Hämta passet
            var shift = await _context.Shifts.FirstOrDefaultAsync(s => s.Id == request.ShiftId, cancellationToken);

            if (shift == null)
            {
                throw new Exception("Arbetspasset kunde inte hittas.");
            }

            // 2. KONTROLLERA TILLGÄNGLIGHET (Här var felet!) 🛠️
            // Vi kastar bara fel om passet INTE är för byte OCH det redan har en ägare.
            // Om UserId är null (öppet pass) så är det fritt fram att ta!
            if (!shift.IsUpForSwap && shift.UserId != null)
            {
                throw new Exception("Detta pass är inte tillgängligt för att tas.");
            }

            // 3. Hämta användaren
            var user = await _context.Users
                .Include(u => u.Shifts)
                .FirstOrDefaultAsync(u => u.Id == request.UserId, cancellationToken);

            if (user == null)
            {
                throw new Exception("Användaren kunde inte hittas.");
            }

            // 4. KROCK-KONTROLL
            // Kontrollera om användaren redan har ett pass på samma dag.
            // (Vi kollar dock inte mot passet vi försöker ta, ifall det av misstag redan står på oss)
            var newShiftDate = shift.StartTime.Date;

            bool hasShiftOnSameDay = user.Shifts.Any(s =>
                s.Id != shift.Id && // Ignorera passet vi försöker ta (om det mot förmodan redan var vårt)
                s.StartTime.Date == newShiftDate
            );

            if (hasShiftOnSameDay)
            {
                throw new Exception("Du kan inte ta ett pass på en dag där du redan har ett annat pass.");
            }

            // 5. UTFÖR UPPDATERINGEN
            shift.UserId = request.UserId;
            shift.IsUpForSwap = false; // Nollställ bytes-flaggan
            shift.User = user;         // Uppdatera navigation property

            // Spara
            await _context.SaveChangesAsync(cancellationToken);

            return true;
        }
    }
}