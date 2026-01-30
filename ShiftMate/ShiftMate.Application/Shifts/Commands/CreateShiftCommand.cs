using FluentValidation;
using MediatR;
using ShiftMate.Application.Interfaces;
using ShiftMate.Domain;
using System.Text.Json.Serialization;

namespace ShiftMate.Application.Shifts.Commands
{
    // 1. DATA
    public record CreateShiftCommand : IRequest<Guid>
    {
        [JsonIgnore]
        public Guid UserId { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
    }

    // 2. LOGIK
    public class CreateShiftHandler : IRequestHandler<CreateShiftCommand, Guid>
    {
        private readonly IAppDbContext _context;
        private readonly IValidator<CreateShiftCommand> _validator; // <--- 1. Vi måste deklarera variabeln här

        // Vi injicerar både databasen OCH validatorn
        public CreateShiftHandler(IAppDbContext context, IValidator<CreateShiftCommand> validator) // <--- 2. Ta emot den här
        {
            _context = context;
            _validator = validator; // <--- 3. Spara den
        }

        public async Task<Guid> Handle(CreateShiftCommand request, CancellationToken cancellationToken)
        {
            // 1. KALLA PÅ ORDNINGSVAKTEN! 👮‍♂️
            var validationResult = await _validator.ValidateAsync(request, cancellationToken);

            if (!validationResult.IsValid)
            {
                // Om reglerna bryts, kasta ett fel
                throw new Exception(validationResult.ToString());
            }

            // 2. Om allt är grönt, kör på som vanligt!
            var shift = new Shift
            {
                Id = Guid.NewGuid(),
                UserId = request.UserId,
                StartTime = request.StartTime,
                EndTime = request.EndTime,
                IsUpForSwap = false
            };

            _context.Shifts.Add(shift);
            await _context.SaveChangesAsync(cancellationToken);

            return shift.Id;
        }
    }
}