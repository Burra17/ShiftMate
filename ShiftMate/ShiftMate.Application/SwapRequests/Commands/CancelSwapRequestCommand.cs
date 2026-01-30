using MediatR;
using Microsoft.EntityFrameworkCore;
using ShiftMate.Application.Interfaces;

namespace ShiftMate.Application.SwapRequests.Commands
{
    // 1. DATA: Vi behöver ID på bytet och ID på den som försöker ta bort det
    public record CancelSwapRequestCommand(Guid SwapRequestId, Guid CurrentUserId) : IRequest;

    // 2. LOGIK
    public class CancelSwapRequestHandler : IRequestHandler<CancelSwapRequestCommand>
    {
        private readonly IAppDbContext _context;

        public CancelSwapRequestHandler(IAppDbContext context)
        {
            _context = context;
        }

        public async Task Handle(CancelSwapRequestCommand request, CancellationToken cancellationToken)
        {
            // A. Hämta förfrågan OCH passet (viktigt med Include!)
            var swapRequest = await _context.SwapRequests
                .Include(sq => sq.Shift)
                .FirstOrDefaultAsync(sq => sq.Id == request.SwapRequestId, cancellationToken);

            // B. Finns den?
            if (swapRequest == null)
            {
                throw new Exception("Hittade inte bytesförfrågan."); // Eller NotFoundException
            }

            // C. SÄKERHETSKOLL: Äger du den här förfrågan? 👮‍♂️
            // Om den som är inloggad INTE är samma person som skapade förfrågan...
            if (swapRequest.RequestingUserId != request.CurrentUserId)
            {
                throw new Exception("Du får inte ta bort någon annans bytesförfrågan!");
            }

            // D. Återställ passet (det är inte längre till salu)
            swapRequest.Shift.IsUpForSwap = false;

            // E. Ta bort förfrågan
            _context.SwapRequests.Remove(swapRequest);

            // F. Spara
            await _context.SaveChangesAsync(cancellationToken);
        }
    }
}