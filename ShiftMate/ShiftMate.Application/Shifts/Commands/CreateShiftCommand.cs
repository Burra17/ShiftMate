using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore; // <--- VIKTIG: Behövs för AnyAsync
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
        private readonly IValidator<CreateShiftCommand> _validator;

        public CreateShiftHandler(IAppDbContext context, IValidator<CreateShiftCommand> validator)
        {
            _context = context;
            _validator = validator;
        }

        public async Task<Guid> Handle(CreateShiftCommand request, CancellationToken cancellationToken)
        {
            // 1. VALIDERING (Datumformat etc.)
            var validationResult = await _validator.ValidateAsync(request, cancellationToken);

            if (!validationResult.IsValid)
            {
                throw new Exception(validationResult.ToString());
            }

            // 2. KROCK-KONTROLL 🛑 (Här är det nya!)
            // Vi kollar om det redan finns ett pass för denna user som överlappar tiden
            var hasOverlap = await _context.Shifts.AnyAsync(s =>
                s.UserId == request.UserId &&
                s.StartTime < request.EndTime &&
                s.EndTime > request.StartTime,
                cancellationToken
            );

            if (hasOverlap)
            {
                // Om krock hittas, avbryt och kasta fel!
                throw new InvalidOperationException("Denna användare har redan ett pass som krockar med den valda tiden.");
            }

            // 3. SKAPA PASSET (Bara om ingen krock fanns)
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