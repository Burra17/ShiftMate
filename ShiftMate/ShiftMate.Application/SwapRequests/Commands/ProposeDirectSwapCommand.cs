using MediatR;
using Microsoft.EntityFrameworkCore;
using ShiftMate.Application.Interfaces;
using ShiftMate.Domain;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Logging;
using System.Globalization;
using Microsoft.Extensions.Configuration;

namespace ShiftMate.Application.SwapRequests.Commands
{
    // Kommandot: Vad som skickas in från Frontend
    public record ProposeDirectSwapCommand : IRequest<Guid>
    {
        public Guid MyShiftId { get; set; }
        public Guid TargetShiftId { get; set; }

        [JsonIgnore]
        public Guid RequestingUserId { get; set; }
    }

    // Handlern: Logiken som utför bytet
    public class ProposeDirectSwapCommandHandler : IRequestHandler<ProposeDirectSwapCommand, Guid>
    {
        private readonly IAppDbContext _context;
        private readonly IEmailService _emailService;
        private readonly ILogger<ProposeDirectSwapCommandHandler> _logger;
        private readonly IConfiguration _configuration;

        public ProposeDirectSwapCommandHandler(
            IAppDbContext context,
            IEmailService emailService,
            ILogger<ProposeDirectSwapCommandHandler> logger,
            IConfiguration configuration)
        {
            _context = context;
            _emailService = emailService;
            _logger = logger;
            _configuration = configuration;
        }

        public async Task<Guid> Handle(ProposeDirectSwapCommand request, CancellationToken cancellationToken)
        {
            // ---------------------------------------------------------
            // 1. HÄMTA DATA
            // ---------------------------------------------------------
            var myShift = await _context.Shifts
                .Include(s => s.User)
                .FirstOrDefaultAsync(s => s.Id == request.MyShiftId, cancellationToken);

            var targetShift = await _context.Shifts
                .Include(s => s.User)
                .FirstOrDefaultAsync(s => s.Id == request.TargetShiftId, cancellationToken);

            var requestingUser = await _context.Users
                .FirstOrDefaultAsync(u => u.Id == request.RequestingUserId, cancellationToken);

            // ---------------------------------------------------------
            // 2. VALIDERING
            // ---------------------------------------------------------
            if (myShift == null || targetShift == null || requestingUser == null)
                throw new Exception("Data saknas.");

            if (myShift.UserId != request.RequestingUserId)
                throw new Exception("Fel ägare.");

            // ---------------------------------------------------------
            // 3. SPARA BYTESFÖRFRÅGAN (Detta sker nu FÖRST ✅)
            // ---------------------------------------------------------
            var swapRequest = new SwapRequest
            {
                Id = Guid.NewGuid(),
                RequestingUserId = request.RequestingUserId,
                ShiftId = myShift.Id,
                TargetUserId = targetShift.UserId,
                TargetShiftId = targetShift.Id,
                Status = "Pending",
                CreatedAt = DateTime.UtcNow
            };

            // Vi lägger till och sparar direkt. Om detta lyckas är bytet "klart".
            await _context.SwapRequests.AddAsync(swapRequest, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);

            // ---------------------------------------------------------
            // 4. FÖRSÖK SKICKA MAIL (Med 2 sekunders timeout)
            // ---------------------------------------------------------
            try
            {
                // Förbered data
                var culture = new CultureInfo("sv-SE");
                var targetDate = targetShift.StartTime.ToString("dddd d MMMM", culture);
                var targetTime = $"{targetShift.StartTime:HH:mm} - {targetShift.EndTime:HH:mm}";
                var myDate = myShift.StartTime.ToString("dddd d MMMM", culture);
                var myTime = $"{myShift.StartTime:HH:mm} - {myShift.EndTime:HH:mm}";

                // Länk
                var baseUrl = _configuration["FrontendUrl"] ?? "http://localhost:5173";
                var actionUrl = $"{baseUrl}/mine";
                var toEmail = targetShift.User.Email;
                var subject = $"Byte? {requestingUser.FirstName} vill byta pass med dig 🔄";

                // HTML-innehåll
                var message = $@"
                    <html>
                    <body style=""font-family: 'Segoe UI', sans-serif; color: #333; background-color: #f4f4f4; padding: 20px;"">
                        <div style=""max-width: 500px; margin: 0 auto; background: #fff; border-radius: 8px; overflow: hidden; box-shadow: 0 4px 10px rgba(0,0,0,0.1);"">
                            <div style=""background-color: #0056b3; padding: 20px; color: white; text-align: center;"">
                                <h2 style=""margin: 0; font-size: 22px;"">Ny bytesförfrågan</h2>
                            </div>
                            <div style=""padding: 25px;"">
                                <p style=""font-size: 16px; margin-bottom: 20px;"">Hej <strong>{targetShift.User.FirstName}</strong>!</p>
                                <p style=""margin-bottom: 25px;"">{requestingUser.FirstName} {requestingUser.LastName} föreslår ett direktbyte med dig.</p>

                                <div style=""background-color: #f8f9fa; border: 1px solid #e9ecef; border-radius: 8px; padding: 15px;"">
                                    <div style=""border-left: 4px solid #dc3545; padding-left: 10px; margin-bottom: 15px;"">
                                        <p style=""margin: 0; font-size: 12px; text-transform: uppercase; color: #6c757d; font-weight: bold;"">Du lämnar</p>
                                        <p style=""margin: 2px 0 0 0; font-size: 16px; font-weight: bold; color: #333;"">{targetDate}</p>
                                        <p style=""margin: 0; font-size: 14px; color: #555;"">Kl. {targetTime}</p>
                                    </div>
                                    <div style=""border-top: 1px dashed #ced4da; margin: 10px 0;""></div>
                                    <div style=""border-left: 4px solid #28a745; padding-left: 10px;"">
                                        <p style=""margin: 0; font-size: 12px; text-transform: uppercase; color: #6c757d; font-weight: bold;"">Du får</p>
                                        <p style=""margin: 2px 0 0 0; font-size: 16px; font-weight: bold; color: #333;"">{myDate}</p>
                                        <p style=""margin: 0; font-size: 14px; color: #555;"">Kl. {myTime}</p>
                                    </div>
                                </div>

                                <div style=""text-align: center; margin-top: 30px; margin-bottom: 10px;"">
                                    <a href=""{actionUrl}"" style=""background-color: #0056b3; color: white; padding: 14px 28px; text-decoration: none; border-radius: 6px; font-weight: bold; font-size: 16px; display: inline-block;"">
                                        Logga in och svara
                                    </a>
                                </div>
                                <p style=""margin-top: 20px; font-size: 12px; color: #999; text-align: center;"">
                                    Länk fungerar inte? Gå till: <a href=""{actionUrl}"" style=""color: #0056b3;"">{actionUrl}</a>
                                </p>
                            </div>
                        </div>
                    </body>
                    </html>";

                // Skicka (med tidsbegränsning så det inte snurrar för evigt på Render Free)
                if (!string.IsNullOrEmpty(toEmail))
                {
                    // 1. Starta mailet (men vänta inte på det än)
                    var emailTask = _emailService.SendEmailAsync(toEmail, subject, message);

                    // 2. Starta en klocka på 2 sekunder
                    var timeoutTask = Task.Delay(2000);

                    // 3. Vänta på den som blir klar först
                    var completedTask = await Task.WhenAny(emailTask, timeoutTask);

                    if (completedTask == timeoutTask)
                    {
                        // Klockan vann = Det tog för lång tid (Render Free Tier blockerar)
                        // Vi loggar bara info och går vidare direkt!
                        Console.WriteLine("[MAIL INFO] Mailet tog för lång tid (Render Free Tier), vi hoppar över det.");
                    }
                    else
                    {
                        // Mailet hann skickas (eller kraschade snabbt)
                        await emailTask;
                    }
                }
            }
            catch (Exception ex)
            {
                // Detta block körs om mailet kraschar snabbt.
                // Vi loggar det som en varning, men kraschar inte appen!
                Console.WriteLine($"[MAIL WARNING] Mail kunde inte skickas: {ex.Message}");
                _logger.LogWarning(ex, "Kunde inte skicka mail till mottagaren, men bytet har sparats i databasen.");
            }

            // ---------------------------------------------------------
            // 5. KLART! Returnera ID
            // ---------------------------------------------------------
            return swapRequest.Id;
        }
    }
}