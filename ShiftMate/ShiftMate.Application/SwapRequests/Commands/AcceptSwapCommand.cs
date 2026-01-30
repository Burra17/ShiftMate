using MediatR;
using ShiftMate.Application.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Text.Json.Serialization; // <--- Behövs för [JsonIgnore]

namespace ShiftMate.Application.SwapRequests.Commands
{
    // 1. HÄR ÄR FIXEN: Vi lägger till CurrentUserId i kommandot
    public record AcceptSwapCommand : IRequest
    {
        public Guid SwapRequestId { get; set; }

        [JsonIgnore] // Vi hämtar detta från token, så Swagger ska inte visa det
        public Guid CurrentUserId { get; set; }
    }

    // 2. HANDLERN (Logiken)
    public class AcceptSwapHandler : IRequestHandler<AcceptSwapCommand>
    {
        private readonly IAppDbContext _context;

        public AcceptSwapHandler(IAppDbContext context)
        {
            _context = context;
        }

        public async Task Handle(AcceptSwapCommand request, CancellationToken cancellationToken)
        {
            // A. Hämta bytet och passet
            var swapRequest = await _context.SwapRequests
                .Include(sr => sr.Shift)
                .FirstOrDefaultAsync(sr => sr.Id == request.SwapRequestId, cancellationToken);

            if (swapRequest == null) throw new Exception("Bytet hittades inte.");
            if (swapRequest.Status != "Pending") throw new Exception("Det här bytet är inte längre tillgängligt.");

            var newShift = swapRequest.Shift;

            // --- NYTT: KROCK-KONTROLL 💥 ---
            // Vi kollar om du har något pass som överlappar med det nya
            var hasOverlap = await _context.Shifts.AnyAsync(s =>
                s.UserId == request.CurrentUserId && // Kolla BARA mina pass
                s.StartTime < newShift.EndTime &&    // Mitt pass börjar innan det nya slutar
                s.EndTime > newShift.StartTime,      // Mitt pass slutar efter det nya börjar
                cancellationToken);

            if (hasOverlap)
            {
                throw new Exception("Du har redan ett pass som krockar med detta!");
            }
            // -------------------------------

            // B. Genomför bytet
            newShift.UserId = request.CurrentUserId;
            newShift.IsUpForSwap = false;
            swapRequest.Status = "Accepted";

            // C. Spara
            await _context.SaveChangesAsync(cancellationToken);
        }
    }
}