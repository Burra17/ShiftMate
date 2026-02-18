using MediatR;
using Microsoft.EntityFrameworkCore;
using ShiftMate.Application.Interfaces;
using ShiftMate.Domain;
using System.Text.Json.Serialization; // <--- 1. VIKTIGT: Lägg till denna

namespace ShiftMate.Application.SwapRequests.Commands
{
    // 1. DATA: Vad behövs? "Vilket pass vill du byta?"
    public record InitiateSwapCommand : IRequest<Guid>
    {
        public Guid ShiftId { get; set; }

        [JsonIgnore] // <--- 2. VIKTIGT: Denna gömmer fältet i Swagger
        public Guid RequestingUserId { get; set; }
    }

    // 2. LOGIK: Validera och skapa
    public class InitiateSwapHandler : IRequestHandler<InitiateSwapCommand, Guid>
    {
        private readonly IAppDbContext _context;

        public InitiateSwapHandler(IAppDbContext context)
        {
            _context = context;
        }

        public async Task<Guid> Handle(InitiateSwapCommand request, CancellationToken cancellationToken)
        {
            // A. Hämta passet
            var shift = await _context.Shifts
                .FirstOrDefaultAsync(s => s.Id == request.ShiftId, cancellationToken);

            if (shift == null)
            {
                throw new Exception("Passet hittades inte.");
            }

            // B. Säkerhetskoll: Äger du verkligen det här passet?
            if (shift.UserId != request.RequestingUserId)
            {
                throw new Exception("Du kan inte byta bort någon annans pass!");
            }

            // C. Skapa förfrågan
            var swapRequest = new SwapRequest
            {
                Id = Guid.NewGuid(),
                ShiftId = request.ShiftId,
                RequestingUserId = request.RequestingUserId,
                Status = SwapRequestStatus.Pending,
                CreatedAt = DateTime.UtcNow
            };

            // D. Markera passet som "Ute för byte"
            shift.IsUpForSwap = true;

            // E. Spara allt
            _context.SwapRequests.Add(swapRequest);
            await _context.SaveChangesAsync(cancellationToken);

            return swapRequest.Id;
        }
    }
}