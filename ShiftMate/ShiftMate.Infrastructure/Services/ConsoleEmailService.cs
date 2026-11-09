using System.Text.RegularExpressions;
using Microsoft.Extensions.Logging;
using ShiftMate.Application.Interfaces;

namespace ShiftMate.Infrastructure.Services;

// E-posttjänst för lokal utveckling utan Resend-nyckel.
// Skickar inga mejl – loggar mottagare, ämne och länkar (t.ex. verifierings- och återställningslänkar)
// i API-terminalen så att flödena kan testas genom att kopiera länken.
public class ConsoleEmailService : IEmailService
{
    private readonly ILogger<ConsoleEmailService> _logger;

    public ConsoleEmailService(ILogger<ConsoleEmailService> logger)
    {
        _logger = logger;
    }

    public Task SendEmailAsync(string toEmail, string subject, string message)
    {
        var links = Regex.Matches(message, "href=\"([^\"]+)\"")
            .Select(m => m.Groups[1].Value)
            .Distinct();

        _logger.LogInformation(
            "E-post skickades INTE (dev utan Resend-nyckel). Till: {To} | Ämne: {Subject} | Länkar: {Links}",
            toEmail, subject, string.Join(" ", links));

        return Task.CompletedTask;
    }
}
