using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using ShiftMate.Application.Interfaces;
using ShiftMate.Domain;

namespace ShiftMate.Application.Shifts.Commands
{
    // 1. DATA
    public record CreateShiftCommand : IRequest<Guid>
    {
        // VIKTIGT: Ingen [JsonIgnore] här, annars kan inte Admin välja person!
        // VIKTIGT: Guid? (nullable) så att vi kan skapa "Öppna pass".
        public Guid? UserId { get; set; }

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
            // 1. VALIDERING
            var validationResult = await _validator.ValidateAsync(request, cancellationToken);

            if (!validationResult.IsValid)
            {
                throw new ValidationException(validationResult.Errors);
            }

            // 2. KROCK-KONTROLL 🛑
            // Vi kollar bara krockar om passet faktiskt ska tilldelas någon (UserId är inte null)
            if (request.UserId.HasValue)
            {
                // Säkerställ först att användaren finns
                var userExists = await _context.Users
                    .AnyAsync(u => u.Id == request.UserId.Value, cancellationToken);

                if (!userExists)
                {
                    // Man kan kasta ValidationException eller NotFound, här kör vi en Exception för enkelhetens skull
                    throw new InvalidOperationException("Användaren hittades inte.");
                }

                // Kolla krockar för just denna användare
                var hasOverlap = await _context.Shifts.AnyAsync(s =>
                    s.UserId == request.UserId &&
                    s.StartTime < request.EndTime &&
                    s.EndTime > request.StartTime,
                    cancellationToken
                );

                if (hasOverlap)
                {
                    throw new InvalidOperationException("Denna användare har redan ett pass som krockar med den valda tiden.");
                }
            }

            // 3. SKAPA PASSET
            var shift = new Shift
            {
                Id = Guid.NewGuid(),
                UserId = request.UserId, // Kan vara null (Öppet pass) eller ett ID (Tilldelat)

                // VIKTIGT: Tvinga tiderna till UTC så PostgreSQL blir nöjd 🌍
                StartTime = DateTime.SpecifyKind(request.StartTime, DateTimeKind.Utc),
                EndTime = DateTime.SpecifyKind(request.EndTime, DateTimeKind.Utc),

                IsUpForSwap = false
            };

            _context.Shifts.Add(shift);
            await _context.SaveChangesAsync(cancellationToken);

            return shift.Id;
        }
    }
}