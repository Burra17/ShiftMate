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
    // Kommandot: Definierar vilken data som krävs för att starta ett direktbyte
    public record ProposeDirectSwapCommand : IRequest<Guid>
    {
        public Guid MyShiftId { get; set; }     // Passet användaren vill bli av med
        public Guid TargetShiftId { get; set; } // Passet användaren vill ha istället

        [JsonIgnore] // UserId sätts oftast i controllern från JWT-token för säkerhet
        public Guid RequestingUserId { get; set; }
    }

    // Handlern: Innehåller affärslogiken för att genomföra bytet
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
            // ------------------------------------------------------------------------------
            // 1. HÄMTA DATA
            // Vi hämtar båda passen. För TargetShift kräver vi att UserId inte är null
            // för att förhindra byten mot "lediga/vakanta" pass.
            // ------------------------------------------------------------------------------
            var myShift = await _context.Shifts
                .Include(s => s.User)
                .FirstOrDefaultAsync(s => s.Id == request.MyShiftId, cancellationToken);

            var targetShift = await _context.Shifts
                .Include(s => s.User)
                .FirstOrDefaultAsync(s => s.Id == request.TargetShiftId && s.UserId != null, cancellationToken);

            var requestingUser = await _context.Users
                .FirstOrDefaultAsync(u => u.Id == request.RequestingUserId, cancellationToken);

            // ------------------------------------------------------------------------------
            // 2. VALIDERING
            // ------------------------------------------------------------------------------
            if (myShift == null || targetShift == null || requestingUser == null)
                throw new Exception("Kunde inte hitta passen eller målanvändaren. Passet kan sakna ägare.");

            if (myShift.UserId != request.RequestingUserId)
                throw new Exception("Du kan bara föreslå byte för pass du själv äger.");

            // ------------------------------------------------------------------------------
            // 3. SPARA BYTESFÖRFRÅGAN (Sker först så att handlingen är säkrad i databasen)
            // ------------------------------------------------------------------------------
            var swapRequest = new SwapRequest
            {
                Id = Guid.NewGuid(),
                RequestingUserId = request.RequestingUserId,
                ShiftId = myShift.Id,
                TargetUserId = targetShift.UserId, // Kollegan som äger det andra passet
                TargetShiftId = targetShift.Id,
                Status = "Pending",
                CreatedAt = DateTime.UtcNow
            };

            await _context.SwapRequests.AddAsync(swapRequest, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);

            // ------------------------------------------------------------------------------
            // 4. FÖRSÖK SKICKA MAIL (Med 2 sekunders "fire-and-forget" logik)
            // ------------------------------------------------------------------------------
            try
            {
                // Formatering för snyggare mail
                var culture = new CultureInfo("sv-SE");
                var targetDate = targetShift.StartTime.ToString("dddd d MMMM", culture);
                var targetTime = $"{targetShift.StartTime:HH:mm} - {targetShift.EndTime:HH:mm}";
                var myDate = myShift.StartTime.ToString("dddd d MMMM", culture);
                var myTime = $"{myShift.StartTime:HH:mm} - {myShift.EndTime:HH:mm}";

                var baseUrl = _configuration["FrontendUrl"] ?? "http://localhost:5173";
                var actionUrl = $"{baseUrl}/mine";
                var toEmail = targetShift.User.Email;
                var subject = $"Byte? {requestingUser.FirstName} vill byta pass med dig 🔄";

                var message = $@"
                    <html>
                    <body style=""font-family: Arial, sans-serif; color: #333;"">
                        <div style=""max-width: 500px; border: 1px solid #eee; padding: 20px;"">
                            <h2 style=""color: #0056b3;"">Ny bytesförfrågan</h2>
                            <p>Hej <strong>{targetShift.User.FirstName}</strong>!</p>
                            <p>{requestingUser.FirstName} {requestingUser.LastName} vill göra ett direktbyte med dig.</p>
                            <hr/>
                            <p><strong>Du lämnar:</strong> {targetDate} ({targetTime})</p>
                            <p><strong>Du får:</strong> {myDate} ({myTime})</p>
                            <hr/>
                            <div style=""margin-top: 20px;"">
                                <a href=""{actionUrl}"" style=""background: #0056b3; color: white; padding: 10px 20px; text-decoration: none; border-radius: 5px;"">Logga in och svara</a>
                            </div>
                        </div>
                    </body>
                    </html>";

                // Timeout-hantering: Om mailservern är seg (t.ex. på Render Free Tier)
                // låter vi inte hela appen vänta mer än 2 sekunder.
                var emailTask = _emailService.SendEmailAsync(toEmail, subject, message);
                var delayTask = Task.Delay(2000);

                var completedTask = await Task.WhenAny(emailTask, delayTask);

                if (completedTask == delayTask)
                {
                    _logger.LogWarning("Mailutskick för byte {Id} tog för lång tid och körs nu i bakgrunden.", swapRequest.Id);
                }
                else
                {
                    await emailTask; // Om mailet blev klart snabbt, awaita det för att fånga ev. fel
                }
            }
            catch (Exception ex)
            {
                // Vi loggar felet men kastar inte Exception, eftersom SwapRequest redan är sparad
                _logger.LogError(ex, "Ett fel uppstod vid mailutskick för byte {Id}, men begäran är sparad.", swapRequest.Id);
            }

            // Returnera ID på den skapade förfrågan
            return swapRequest.Id;
        }
    }
}