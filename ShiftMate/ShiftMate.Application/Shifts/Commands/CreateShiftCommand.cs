using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using ShiftMate.Application.Interfaces;
using ShiftMate.Domain;
using Microsoft.Extensions.Logging;

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
        private readonly IEmailService _emailService;
        private readonly Microsoft.Extensions.Logging.ILogger<CreateShiftHandler> _logger;

        public CreateShiftHandler(
            IAppDbContext context,
            IValidator<CreateShiftCommand> validator,
            IEmailService emailService,
            Microsoft.Extensions.Logging.ILogger<CreateShiftHandler> logger)
        {
            _context = context;
            _validator = validator;
            _emailService = emailService;
            _logger = logger;
        }

        public async Task<Guid> Handle(CreateShiftCommand request, CancellationToken cancellationToken)
        {
            // Normalisera tiderna till UTC direkt — Npgsql 8 kräver DateTimeKind.Utc
            // för alla queries mot timestamptz-kolumner i PostgreSQL
            var startTimeUtc = DateTime.SpecifyKind(request.StartTime, DateTimeKind.Utc);
            var endTimeUtc = DateTime.SpecifyKind(request.EndTime, DateTimeKind.Utc);

            // 1. VALIDERING
            var validationResult = await _validator.ValidateAsync(request, cancellationToken);

            if (!validationResult.IsValid)
            {
                throw new ValidationException(validationResult.Errors);
            }

            // 2. KROCK-KONTROLL
            // Vi kollar bara krockar om passet faktiskt ska tilldelas någon (UserId är inte null)
            Domain.User? assignedUser = null;
            if (request.UserId.HasValue)
            {
                // Hämta användaren (behövs för både krock-kontroll och email)
                assignedUser = await _context.Users
                    .FirstOrDefaultAsync(u => u.Id == request.UserId.Value, cancellationToken);

                if (assignedUser == null)
                {
                    throw new InvalidOperationException("Användaren hittades inte.");
                }

                // Kolla krockar för just denna användare
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
                IsUpForSwap = false
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

                    var subject = $"📅 Nytt pass tilldelat: {shiftDate}";
                    var emailBody = $@"
                        <html>
                        <body style=""font-family: Arial, sans-serif; color: #333;"">
                            <div style=""max-width: 500px; border: 1px solid #eee; padding: 20px;"">
                                <h2 style=""color: #0056b3;"">Nytt pass tilldelat</h2>
                                <p>Hej <strong>{assignedUser.FirstName}</strong>!</p>
                                <p>En administratör har tilldelat dig ett nytt arbetspass.</p>
                                <hr/>
                                <p><strong>Datum:</strong> {shiftDate}</p>
                                <p><strong>Tid:</strong> {shiftTime}</p>
                                <p><strong>Längd:</strong> {duration:F1} timmar</p>
                                <hr/>
                                <p style=""color: #666; font-size: 12px;"">Logga in på ShiftMate för att se ditt uppdaterade schema.</p>
                            </div>
                        </body>
                        </html>";

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