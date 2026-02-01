using MediatR;
using Microsoft.EntityFrameworkCore;
using ShiftMate.Application.Interfaces;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace ShiftMate.Application.Shifts.Commands
{
    public class CancelShiftSwapCommandHandler : IRequestHandler<CancelShiftSwapCommand, bool>
    {
        private readonly IAppDbContext _context;

        public CancelShiftSwapCommandHandler(IAppDbContext context)
        {
            _context = context;
        }

        public async Task<bool> Handle(CancelShiftSwapCommand request, CancellationToken cancellationToken)
        {
            // Hämta det specifika arbetspasset från databasen.
            var shift = await _context.Shifts.FirstOrDefaultAsync(s => s.Id == request.ShiftId, cancellationToken);

            // Kontrollera att passet existerar.
            if (shift == null)
            {
                throw new Exception("Arbetspasset kunde inte hittas.");
            }

            // Kontrollera att det är ägaren av passet som försöker ångra.
            if (shift.UserId != request.UserId)
            {
                throw new Exception("Du kan inte ångra ett pass som inte är ditt.");
            }

            // Kontrollera att passet faktiskt är ute för byte.
            if (!shift.IsUpForSwap)
            {
                throw new Exception("Detta pass är inte markerat som ledigt för byte.");
            }

            // Återställ passet till att inte vara uppe för byte.
            shift.IsUpForSwap = false;

            // Spara ändringarna i databasen.
            await _context.SaveChangesAsync(cancellationToken);

            return true;
        }
    }
}
