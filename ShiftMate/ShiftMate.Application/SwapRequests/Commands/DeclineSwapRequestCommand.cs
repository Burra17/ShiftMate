using MediatR;
using Microsoft.EntityFrameworkCore;
using ShiftMate.Application.Interfaces;
using ShiftMate.Domain;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Logging;

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
        private readonly IEmailService _emailService;
        private readonly Microsoft.Extensions.Logging.ILogger<DeclineSwapRequestCommandHandler> _logger;

        public DeclineSwapRequestCommandHandler(
            IAppDbContext context,
            IEmailService emailService,
            Microsoft.Extensions.Logging.ILogger<DeclineSwapRequestCommandHandler> logger)
        {
            _context = context;
            _emailService = emailService;
            _logger = logger;
        }

        public async Task Handle(DeclineSwapRequestCommand request, CancellationToken cancellationToken)
        {
            // 1. Hämta förfrågan från databasen (med relaterad data för email).
            var swapRequest = await _context.SwapRequests
                .Include(sr => sr.RequestingUser)
                .Include(sr => sr.TargetUser)
                .Include(sr => sr.Shift)
                .Include(sr => sr.TargetShift)
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
            if (swapRequest.Status != SwapRequestStatus.Pending)
            {
                throw new Exception("Denna förfrågan är inte längre aktiv och kan inte nekas.");
            }

            // 5. Uppdatera status till "Declined".
            swapRequest.Status = SwapRequestStatus.Declined;

            // 6. Spara ändringarna.
            await _context.SaveChangesAsync(cancellationToken);

            // 7. Skicka email till den som föreslog bytet (fire-and-forget)
            try
            {
                if (swapRequest.RequestingUser != null && swapRequest.TargetUser != null && swapRequest.Shift != null)
                {
                    var culture = new System.Globalization.CultureInfo("sv-SE");
                    var subject = $"❌ {swapRequest.TargetUser.FirstName} nekade bytet";
                    var emailBody = Services.EmailTemplateService.SwapDeclined(
                        swapRequest.RequestingUser.FirstName,
                        $"{swapRequest.TargetUser.FirstName} {swapRequest.TargetUser.LastName}",
                        swapRequest.Shift.StartTime.ToString("dddd d MMMM", culture),
                        $"{swapRequest.Shift.StartTime:HH:mm} - {swapRequest.Shift.EndTime:HH:mm}"
                    );

                    _ = Task.Run(async () =>
                    {
                        try
                        {
                            await _emailService.SendEmailAsync(
                                swapRequest.RequestingUser.Email,
                                subject,
                                emailBody
                            );
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, "Kunde inte skicka avslag-email till {Email}", swapRequest.RequestingUser.Email);
                        }
                    });
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Email-notifiering misslyckades för nekat byte {Id}", swapRequest.Id);
            }
        }
    }
}
