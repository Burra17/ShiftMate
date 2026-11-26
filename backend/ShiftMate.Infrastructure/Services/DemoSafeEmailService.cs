using Microsoft.Extensions.Logging;
using ShiftMate.Application.Interfaces;

namespace ShiftMate.Infrastructure.Services;

// Omslag (decorator) runt den riktiga e-posttjänsten: mejl till demo-användare skickas aldrig.
// Demo-användare har adresser på den reserverade toppdomänen .invalid (DemoConstants.EmailDomain),
// som aldrig kan finnas på riktigt – alla .invalid-adresser hoppas över.
// Ett ställe istället för en kontroll i varje handler – nya mejlflöden täcks automatiskt.
public class DemoSafeEmailService : IEmailService
{
    private readonly IEmailService _inner;
    private readonly ILogger<DemoSafeEmailService> _logger;

    public DemoSafeEmailService(IEmailService inner, ILogger<DemoSafeEmailService> logger)
    {
        _inner = inner;
        _logger = logger;
    }

    public Task SendEmailAsync(string toEmail, string subject, string message)
    {
        if (toEmail.EndsWith(".invalid", StringComparison.OrdinalIgnoreCase))
        {
            _logger.LogInformation("Hoppar över mejl till demo-adress: {Subject}", subject);
            return Task.CompletedTask;
        }

        return _inner.SendEmailAsync(toEmail, subject, message);
    }
}
