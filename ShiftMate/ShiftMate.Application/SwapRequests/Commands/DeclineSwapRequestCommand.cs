using MediatR;
using Microsoft.EntityFrameworkCore;
using ShiftMate.Application.Interfaces;
using System.Text.Json.Serialization;

namespace ShiftMate.Application.SwapRequests.Commands
{
    // Svensk kommentar: Datan som behövs för att neka en bytesförfrågan.
    // SwapRequestId kommer från URL:en, och CurrentUserId sätts i controllern.
    public record DeclineSwapRequestCommand : IRequest
    {
        public Guid SwapRequestId { get; set; }

        [JsonIgnore]
        public Guid CurrentUserId { get; set; }
    }

    // Svensk kommentar: Handläggaren som utför logiken för att neka förfrågan.
    public class DeclineSwapRequestCommandHandler : IRequestHandler<DeclineSwapRequestCommand>
    {
        private readonly IAppDbContext _context;

        public DeclineSwapRequestCommandHandler(IAppDbContext context)
        {
            _context = context;
        }

        public async Task Handle(DeclineSwapRequestCommand request, CancellationToken cancellationToken)
        {
            // 1. Hämta förfrågan från databasen.
            var swapRequest = await _context.SwapRequests
                .FirstOrDefaultAsync(sr => sr.Id == request.SwapRequestId, cancellationToken);

            // 2. Validera att förfrågan existerar.
            if (swapRequest == null)
            {
                throw new Exception("Bytesförfrågan kunde inte hittas.");
            }

            // 3. Säkerhetskontroll: Endast mottagaren får neka.
            if (swapRequest.TargetUserId != request.CurrentUserId)
            {
                throw new Exception("Du har inte behörighet att neka denna förfrågan.");
            }

            // 4. Validera att förfrågan fortfarande är aktiv.
            if (swapRequest.Status != "Pending")
            {
                throw new Exception("Denna förfrågan är inte längre aktiv och kan inte nekas.");
            }

            // 5. Uppdatera status till "Declined".
            swapRequest.Status = "Declined";

            // 6. Spara ändringarna.
            await _context.SaveChangesAsync(cancellationToken);
        }
    }
}
