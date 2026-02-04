using MediatR;
using Microsoft.EntityFrameworkCore;
using ShiftMate.Application.Interfaces;
using ShiftMate.Domain;
using System.Security.Claims;
using System.Text.Json.Serialization;

namespace ShiftMate.Application.SwapRequests.Commands
{
    // Svensk kommentar: Datan som behövs för att föreslå ett direktbyte.
    // "Jag (RequestingUserId) vill byta mitt pass (MyShiftId) mot ett annat pass (TargetShiftId)."
    public record ProposeDirectSwapCommand : IRequest<Guid>
    {
        public Guid MyShiftId { get; set; }
        public Guid TargetShiftId { get; set; }

        [JsonIgnore]
        public Guid RequestingUserId { get; set; }
    }

    // Svensk kommentar: Handläggaren som utför logiken för att skapa bytesförslaget.
    public class ProposeDirectSwapCommandHandler : IRequestHandler<ProposeDirectSwapCommand, Guid>
    {
        private readonly IAppDbContext _context;

        public ProposeDirectSwapCommandHandler(IAppDbContext context)
        {
            _context = context;
        }

        public async Task<Guid> Handle(ProposeDirectSwapCommand request, CancellationToken cancellationToken)
        {
            // 1. Hämta båda passen från databasen.
            // Inkludera User för att kunna sätta TargetUserId
            var myShift = await _context.Shifts
                .Include(s => s.User)
                .FirstOrDefaultAsync(s => s.Id == request.MyShiftId, cancellationToken);
            var targetShift = await _context.Shifts
                .Include(s => s.User)
                .FirstOrDefaultAsync(s => s.Id == request.TargetShiftId, cancellationToken);

            // 2. Validera att passen existerar.
            if (myShift == null)
            {
                throw new Exception("Ditt pass kunde inte hittas.");
            }
            if (targetShift == null)
            {
                throw new Exception("Passet du vill byta till kunde inte hittas.");
            }

            // 3. Validera ägarskap och logik.
            if (myShift.UserId != request.RequestingUserId)
            {
                throw new Exception("Du kan bara byta bort dina egna pass.");
            }
            if (myShift.Id == targetShift.Id)
            {
                throw new Exception("Du kan inte byta ett pass med sig självt.");
            }
            if (targetShift.UserId == request.RequestingUserId)
            {
                throw new Exception("Du kan inte byta pass med dig själv.");
            }

            // 4. Skapa en ny SwapRequest.
            var swapRequest = new SwapRequest
            {
                RequestingUserId = request.RequestingUserId,
                ShiftId = myShift.Id, // Passet som erbjuds

                TargetUserId = targetShift.UserId, // Användaren som äger målpasset
                TargetShiftId = targetShift.Id, // Passet som efterfrågas

                Status = "Pending",
                CreatedAt = DateTime.UtcNow
            };

            await _context.SwapRequests.AddAsync(swapRequest, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);

            return swapRequest.Id;
        }
    }
}
