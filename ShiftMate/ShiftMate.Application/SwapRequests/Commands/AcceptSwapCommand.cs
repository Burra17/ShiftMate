using MediatR;
using Microsoft.EntityFrameworkCore;
using ShiftMate.Application.Interfaces;

namespace ShiftMate.Application.SwapRequests.Commands
{
    // 1. DATA: Vad behövs? "Vilket byte gäller det och vem tar över?"
    public record AcceptSwapCommand : IRequest<bool>
    {
        public Guid SwapRequestId { get; set; }
        public Guid NewUserId { get; set; }
    }

    // 2. LOGIK
    public class AcceptSwapHandler : IRequestHandler<AcceptSwapCommand, bool>
    {
        private readonly IAppDbContext _context;

        public AcceptSwapHandler(IAppDbContext context)
        {
            _context = context;
        }

        public async Task<bool> Handle(AcceptSwapCommand request, CancellationToken cancellationToken)
        {
            // A. Hämta bytesförfrågan OCH passet det gäller
            var swapRequest = await _context.SwapRequests
                .Include(sq => sq.Shift)
                .FirstOrDefaultAsync(sq => sq.Id == request.SwapRequestId, cancellationToken);

            // B. Validering
            if (swapRequest == null)
                throw new Exception("Hittade inte bytesförfrågan.");

            if (swapRequest.Status != "Pending")
                throw new Exception("Det här bytet är inte längre tillgängligt.");

            // C. GENOMFÖR BYTET (Här händer magin!)
            // 1. Byt ägare på passet
            swapRequest.Shift.UserId = request.NewUserId;
            // 2. Markera att passet inte längre är till salu
            swapRequest.Shift.IsUpForSwap = false;
            // 3. Uppdatera status på förfrågan
            swapRequest.Status = "Approved";

            // D. Spara
            await _context.SaveChangesAsync(cancellationToken);

            return true;
        }
    }
}