using MediatR;
using ShiftMate.Application.Interfaces;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using ShiftMate.Domain;
using System;

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
            // Hämta det specifika arbetspasset från databasen.
            var shift = await _context.Shifts.FirstOrDefaultAsync(s => s.Id == request.ShiftId, cancellationToken);

            // Om passet inte finns, kasta ett fel.
            if (shift == null)
            {
                throw new Exception("Arbetspasset kunde inte hittas.");
            }

            // Kontrollera om passet faktiskt är markerat som ledigt.
            if (!shift.IsUpForSwap)
            {
                throw new Exception("Detta pass är inte tillgängligt för att tas.");
            }
            
            // Hämta användaren som försöker ta passet, inkludera deras befintliga pass.
            var user = await _context.Users
                .Include(u => u.Shifts) // Inkludera passen för att kunna kontrollera dem.
                .FirstOrDefaultAsync(u => u.Id == request.UserId, cancellationToken);
            if (user == null)
            {
                throw new Exception("Användaren kunde inte hittas.");
            }

            // Kontrollera om användaren redan har ett pass på samma dag.
            var newShiftDate = shift.StartTime.Date;
            if (user.Shifts.Any(s => s.StartTime.Date == newShiftDate))
            {
                throw new Exception("Du kan inte ta ett pass på en dag där du redan har ett annat pass.");
            }

            // Tilldela passet till den nya användaren.
            shift.UserId = request.UserId;
            shift.IsUpForSwap = false; // Markera passet som upptaget.
            shift.User = user; // Uppdatera navigation property.

            // Spara ändringarna i databasen.
            await _context.SaveChangesAsync(cancellationToken);

            return true;
        }
    }
}
