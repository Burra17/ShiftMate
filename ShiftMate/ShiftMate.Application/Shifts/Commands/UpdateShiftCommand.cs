using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using ShiftMate.Application.Interfaces;

namespace ShiftMate.Application.Shifts.Commands
{
    // 1. DATA
    public record UpdateShiftCommand : IRequest<bool>
    {
        public Guid ShiftId { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public Guid? UserId { get; set; }
    }

    // 2. LOGIK
    public class UpdateShiftHandler : IRequestHandler<UpdateShiftCommand, bool>
    {
        private readonly IAppDbContext _context;
        private readonly IValidator<UpdateShiftCommand> _validator;

        public UpdateShiftHandler(IAppDbContext context, IValidator<UpdateShiftCommand> validator)
        {
            _context = context;
            _validator = validator;
        }

        public async Task<bool> Handle(UpdateShiftCommand request, CancellationToken cancellationToken)
        {
            // Normalisera tiderna till UTC direkt — Npgsql 8 kräver DateTimeKind.Utc
            var startTimeUtc = DateTime.SpecifyKind(request.StartTime, DateTimeKind.Utc);
            var endTimeUtc = DateTime.SpecifyKind(request.EndTime, DateTimeKind.Utc);

            // 1. VALIDERING
            var validationResult = await _validator.ValidateAsync(request, cancellationToken);
            if (!validationResult.IsValid)
            {
                throw new ValidationException(validationResult.Errors);
            }

            // 2. HÄMTA PASSET
            var shift = await _context.Shifts
                .FirstOrDefaultAsync(s => s.Id == request.ShiftId, cancellationToken);

            if (shift == null)
            {
                throw new InvalidOperationException("Passet hittades inte.");
            }

            // 3. KROCK-KONTROLL (om passet tilldelas en användare)
            if (request.UserId.HasValue)
            {
                var user = await _context.Users
                    .FirstOrDefaultAsync(u => u.Id == request.UserId.Value, cancellationToken);

                if (user == null)
                {
                    throw new InvalidOperationException("Användaren hittades inte.");
                }

                // Kolla krockar — exkludera det egna passet från kontrollen
                var hasOverlap = await _context.Shifts.AnyAsync(s =>
                    s.Id != request.ShiftId &&
                    s.UserId == request.UserId &&
                    s.StartTime < endTimeUtc &&
                    s.EndTime > startTimeUtc,
                    cancellationToken
                );

                if (hasOverlap)
                {
                    throw new InvalidOperationException("Denna användare har redan ett pass som krockar med den valda tiden.");
                }
            }

            // 4. UPPDATERA PASSET
            shift.StartTime = startTimeUtc;
            shift.EndTime = endTimeUtc;
            shift.UserId = request.UserId;

            await _context.SaveChangesAsync(cancellationToken);

            return true;
        }
    }
}
