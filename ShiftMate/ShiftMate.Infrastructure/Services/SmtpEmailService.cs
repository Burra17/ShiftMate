using ShiftMate.Application.Interfaces;
using System.Net.Mail;
using System.Net;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;

namespace ShiftMate.Infrastructure.Services
{
    // Konfigurationsklass för e-postinställningar
    public class EmailSettings
    {
        public string Host { get; set; } = string.Empty;
        public int Port { get; set; }
        public string Username { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string FromEmail { get; set; } = string.Empty; // Avsändarens e-postadress
        public bool EnableSsl { get; set; } = true; // Standardvärde
    }

    // Implementering av e-posttjänsten med SMTP
    public class SmtpEmailService : IEmailService
    {
        private readonly EmailSettings? _emailSettings; // E-postinställningar
        private readonly ILogger<SmtpEmailService> _logger; // Logger för felhantering och information

        // Konstruktor som tar in konfiguration och logger
        public SmtpEmailService(IConfiguration configuration, ILogger<SmtpEmailService> logger)
        {
            _emailSettings = configuration.GetSection("EmailSettings").Get<EmailSettings>();

            // Validera att inställningarna laddades korrekt
            if (_emailSettings == null)
            {
                throw new ArgumentNullException(nameof(EmailSettings), "EmailSettings-sektionen saknas eller är felaktig i konfigurationen.");
            }
            if (string.IsNullOrEmpty(_emailSettings.Host))
            {
                throw new ArgumentNullException(nameof(_emailSettings.Host), "EmailSettings: Host saknas i konfigurationen.");
            }
            if (string.IsNullOrEmpty(_emailSettings.Username))
            {
                throw new ArgumentNullException(nameof(_emailSettings.Username), "EmailSettings: Username saknas i konfigurationen.");
            }
            if (string.IsNullOrEmpty(_emailSettings.Password))
            {
                throw new ArgumentNullException(nameof(_emailSettings.Password), "EmailSettings: Password saknas i konfigurationen.");
            }
            if (string.IsNullOrEmpty(_emailSettings.FromEmail))
            {
                throw new ArgumentNullException(nameof(_emailSettings.FromEmail), "EmailSettings: FromEmail saknas i konfigurationen.");
            }

            _logger = logger;
        }

        // Metod för att skicka e-post asynkront
        public async Task SendEmailAsync(string toEmail, string subject, string message)
        {
            if (_emailSettings == null)
            {
                _logger.LogError("E-postinställningar saknas. Kan inte skicka e-post.");
                return;
            }
            // Skapa och konfigurera SMTP-klienten
            using (var client = new SmtpClient(_emailSettings.Host, _emailSettings.Port))
            {
                client.EnableSsl = _emailSettings.EnableSsl;
                client.UseDefaultCredentials = false;
                client.Credentials = new NetworkCredential(_emailSettings.Username, _emailSettings.Password);
                client.DeliveryMethod = SmtpDeliveryMethod.Network;

                var mailMessage = new MailMessage
                {
                    From = new MailAddress(_emailSettings.FromEmail),
                    Subject = subject,
                    Body = message,
                    IsBodyHtml = true // Vi kommer att använda HTML för snyggare mail
                };
                mailMessage.To.Add(toEmail);

                try
                {
                    await client.SendMailAsync(mailMessage);
                    _logger.LogInformation("E-post skickat till {ToEmail} med ämne: {Subject}", toEmail, subject);
                }
                catch (SmtpException ex)
                {
                    _logger.LogError(ex, "SMTP-fel vid sändning av e-post till {ToEmail}. Felkod: {SmtpStatusCode}, Meddelande: {Message}", toEmail, ex.StatusCode, ex.Message);
                    // Kasta inte om felet, eftersom kravet är att inte krascha hela anropet.
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Okänt fel vid sändning av e-post till {ToEmail}.", toEmail);
                    // Kasta inte om felet, eftersom kravet är att inte krascha hela anropet.
                }
            }
        }
    }
}
