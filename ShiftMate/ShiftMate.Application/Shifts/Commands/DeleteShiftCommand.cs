using MediatR;
using Microsoft.EntityFrameworkCore;
using ShiftMate.Application.Interfaces;
using ShiftMate.Domain;

namespace ShiftMate.Application.Shifts.Commands
{
    // 1. DATA
    public record DeleteShiftCommand(Guid ShiftId) : IRequest<bool>;

    // 2. LOGIK
    public class DeleteShiftHandler : IRequestHandler<DeleteShiftCommand, bool>
    {
        private readonly IAppDbContext _context;

        public DeleteShiftHandler(IAppDbContext context)
        {
            _context = context;
        }

        public async Task<bool> Handle(DeleteShiftCommand request, CancellationToken cancellationToken)
        {
            // 1. HÄMTA PASSET med tillhörande bytesförfrågningar
            var shift = await _context.Shifts
                .Include(s => s.SwapRequests)
                .FirstOrDefaultAsync(s => s.Id == request.ShiftId, cancellationToken);

            if (shift == null)
            {
                throw new InvalidOperationException("Passet hittades inte.");
            }

            // 2. AVBRYT ALLA VÄNTANDE BYTESFÖRFRÅGNINGAR och ta bort dem
            foreach (var swapRequest in shift.SwapRequests.ToList())
            {
                if (swapRequest.Status == SwapRequestStatus.Pending)
                {
                    swapRequest.Status = SwapRequestStatus.Cancelled;
                }
                _context.SwapRequests.Remove(swapRequest);
            }

            // 3. TA BORT PASSET
            _context.Shifts.Remove(shift);
            await _context.SaveChangesAsync(cancellationToken);

            return true;
        }
    }
}
