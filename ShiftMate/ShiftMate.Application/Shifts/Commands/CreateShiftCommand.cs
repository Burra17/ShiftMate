using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using ShiftMate.Application.Interfaces;
using ShiftMate.Domain;
using Microsoft.Extensions.Logging;
using System.Text.Json.Serialization;

namespace ShiftMate.Application.Shifts.Commands
{
    // 1. DATA
    public record CreateShiftCommand : IRequest<Guid>
    {
        public Guid? UserId { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }

        [JsonIgnore]
        public Guid OrganizationId { get; set; }
    }

    // 2. LOGIK
    public class CreateShiftHandler : IRequestHandler<CreateShiftCommand, Guid>
    {
        private readonly IAppDbContext _context;
        private readonly IValidator<CreateShiftCommand> _validator;
        private readonly IEmailService _emailService;
        private readonly ILogger<CreateShiftHandler> _logger;

        public CreateShiftHandler(
            IAppDbContext context,
            IValidator<CreateShiftCommand> validator,
            IEmailService emailService,
            ILogger<CreateShiftHandler> logger)
        {
            _context = context;
            _validator = validator;
            _emailService = emailService;
            _logger = logger;
        }

        public async Task<Guid> Handle(CreateShiftCommand request, CancellationToken cancellationToken)
        {
            var startTimeUtc = DateTime.SpecifyKind(request.StartTime, DateTimeKind.Utc);
            var endTimeUtc = DateTime.SpecifyKind(request.EndTime, DateTimeKind.Utc);

            // 1. VALIDERING
            var validationResult = await _validator.ValidateAsync(request, cancellationToken);
            if (!validationResult.IsValid)
            {
                throw new ValidationException(validationResult.Errors);
            }

            // 2. KROCK-KONTROLL
            Domain.User? assignedUser = null;
            if (request.UserId.HasValue)
            {
                assignedUser = await _context.Users
                    .FirstOrDefaultAsync(u => u.Id == request.UserId.Value, cancellationToken);

                if (assignedUser == null)
                {
                    throw new InvalidOperationException("Användaren hittades inte.");
                }

                // Validera att användaren tillhör samma organisation
                if (assignedUser.OrganizationId != request.OrganizationId)
                {
                    throw new InvalidOperationException("Användaren tillhör inte samma organisation.");
                }

                var hasOverlap = await _context.Shifts.AnyAsync(s =>
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

            // 3. SKAPA PASSET
            var shift = new Shift
            {
                Id = Guid.NewGuid(),
                UserId = request.UserId,
                StartTime = startTimeUtc,
                EndTime = endTimeUtc,
                IsUpForSwap = false,
                OrganizationId = request.OrganizationId
            };

            _context.Shifts.Add(shift);
            await _context.SaveChangesAsync(cancellationToken);

            // 4. SKICKA EMAIL om passet tilldelades en specifik användare
            if (assignedUser != null)
            {
                try
                {
                    var culture = new System.Globalization.CultureInfo("sv-SE");
                    var shiftDate = startTimeUtc.ToString("dddd d MMMM", culture);
                    var shiftTime = $"{startTimeUtc:HH:mm} - {endTimeUtc:HH:mm}";
                    var duration = (endTimeUtc - startTimeUtc).TotalHours;

                    var subject = $"Nytt pass tilldelat: {shiftDate}";
                    var emailBody = Services.EmailTemplateService.ShiftAssigned(
                        assignedUser.FirstName,
                        shiftDate,
                        shiftTime,
                        duration
                    );

                    _ = Task.Run(async () =>
                    {
                        try
                        {
                            await _emailService.SendEmailAsync(assignedUser.Email, subject, emailBody);
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, "Kunde inte skicka pass-tilldelnings-email till {Email}", assignedUser.Email);
                        }
                    });
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Email-notifiering misslyckades för nytt pass {Id}", shift.Id);
                }
            }

            return shift.Id;
        }
    }
}
