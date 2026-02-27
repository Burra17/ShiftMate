using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using ShiftMate.Application.Interfaces;
using ShiftMate.Application.Services;
using System.Security.Cryptography;

namespace ShiftMate.Application.Users.Commands
{
    // Command för att begära lösenordsåterställning via e-post
    public record ForgotPasswordCommand : IRequest
    {
        public string Email { get; set; } = string.Empty;
    }

    // Handler som genererar en återställningstoken och skickar e-post
    public class ForgotPasswordHandler : IRequestHandler<ForgotPasswordCommand>
    {
        private readonly IAppDbContext _context;
        private readonly IEmailService _emailService;
        private readonly ILogger<ForgotPasswordHandler> _logger;

        public ForgotPasswordHandler(
            IAppDbContext context,
            IEmailService emailService,
            ILogger<ForgotPasswordHandler> logger)
        {
            _context = context;
            _emailService = emailService;
            _logger = logger;
        }

        public async Task Handle(ForgotPasswordCommand request, CancellationToken cancellationToken)
        {
            // 1. Hitta användaren (skiftlägesokänsligt)
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Email == request.Email.ToLowerInvariant(), cancellationToken);

            // Anti-enumeration: returnera tyst om användaren inte finns
            if (user == null) return;

            // 2. Generera säker token (64 bytes = 512 bits)
            var tokenBytes = RandomNumberGenerator.GetBytes(64);
            var token = Convert.ToBase64String(tokenBytes)
                .Replace("+", "-")
                .Replace("/", "_")
                .TrimEnd('=');

            // 3. Spara BCrypt-hash av token + utgångstid (1 timme)
            user.ResetTokenHash = BCrypt.Net.BCrypt.HashPassword(token);
            user.ResetTokenExpiresAt = DateTime.UtcNow.AddHours(1);
            await _context.SaveChangesAsync(cancellationToken);

            // 4. Skicka e-post med återställningslänk
            try
            {
                var encodedEmail = Uri.EscapeDataString(user.Email);
                var encodedToken = Uri.EscapeDataString(token);
                var resetPath = $"/reset-password?token={encodedToken}&email={encodedEmail}";

                var subject = "Återställ ditt lösenord — ShiftMate";
                var emailBody = EmailTemplateService.PasswordReset(user.FirstName, resetPath);

                await _emailService.SendEmailAsync(user.Email, subject, emailBody);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Kunde inte skicka återställnings-email till {Email}", user.Email);
            }
        }
    }
}
