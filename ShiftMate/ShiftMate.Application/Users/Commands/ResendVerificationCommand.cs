using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using ShiftMate.Application.Interfaces;
using ShiftMate.Application.Services;
using System.Security.Cryptography;

namespace ShiftMate.Application.Users.Commands
{
    public record ResendVerificationCommand : IRequest
    {
        public string Email { get; set; } = string.Empty;
    }

    public class ResendVerificationHandler : IRequestHandler<ResendVerificationCommand>
    {
        private readonly IAppDbContext _context;
        private readonly IEmailService _emailService;
        private readonly ILogger<ResendVerificationHandler> _logger;

        public ResendVerificationHandler(
            IAppDbContext context,
            IEmailService emailService,
            ILogger<ResendVerificationHandler> logger)
        {
            _context = context;
            _emailService = emailService;
            _logger = logger;
        }

        public async Task Handle(ResendVerificationCommand request, CancellationToken cancellationToken)
        {
            // 1. Hitta användaren
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Email == request.Email.ToLowerInvariant(), cancellationToken);

            // Anti-enumeration: returnera tyst
            if (user == null) return;

            // Redan verifierad — inget att göra
            if (user.IsEmailVerified) return;

            // 2. Generera ny token
            var tokenBytes = RandomNumberGenerator.GetBytes(64);
            var token = Convert.ToBase64String(tokenBytes)
                .Replace("+", "-")
                .Replace("/", "_")
                .TrimEnd('=');

            // 3. Spara hash + utgångstid (24 timmar)
            user.EmailVerificationTokenHash = BCrypt.Net.BCrypt.HashPassword(token);
            user.EmailVerificationTokenExpiresAt = DateTime.UtcNow.AddHours(24);
            await _context.SaveChangesAsync(cancellationToken);

            // 4. Skicka verifieringsmail
            try
            {
                var encodedEmail = Uri.EscapeDataString(user.Email);
                var encodedToken = Uri.EscapeDataString(token);
                var verifyPath = $"/verify-email?token={encodedToken}&email={encodedEmail}";

                var subject = "Verifiera din e-post — ShiftMate";
                var emailBody = EmailTemplateService.EmailVerification(user.FirstName, verifyPath);

                await _emailService.SendEmailAsync(user.Email, subject, emailBody);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Kunde inte skicka verifieringsmail till {Email}", user.Email);
            }
        }
    }
}
