using MediatR;
using ShiftMate.Application.Interfaces;
using ShiftMate.Domain;
using System.Text.Json.Serialization;

namespace ShiftMate.Application.Shifts.Commands
{
    // 1. DATA: Vad behövs för att skapa ett pass?
    // Vi returnerar Guid (det nya passets ID)
    public record CreateShiftCommand : IRequest<Guid>
    {
        [JsonIgnore] // Vi vill inte att klienten skickar med detta
        public Guid UserId { get; set; } // Vem ska jobba?
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
    }

    // 2. LOGIK: Hur sparar vi det?
    public class CreateShiftHandler : IRequestHandler<CreateShiftCommand, Guid>
    {
        private readonly IAppDbContext _context;

        public CreateShiftHandler(IAppDbContext context)
        {
            _context = context;
        }

        public async Task<Guid> Handle(CreateShiftCommand request, CancellationToken cancellationToken)
        {
            // Skapa entiteten
            var shift = new Shift
            {
                Id = Guid.NewGuid(),
                UserId = request.UserId,
                StartTime = request.StartTime,
                EndTime = request.EndTime,
                IsUpForSwap = false // Nytt pass är inte till salu direkt
            };

            // Lägg till i databasen
            _context.Shifts.Add(shift);

            // Spara ändringar
            await _context.SaveChangesAsync(cancellationToken);

            return shift.Id;
        }
    }
}