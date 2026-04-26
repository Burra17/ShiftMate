using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using ShiftMate.Application.Interfaces;
using ShiftMate.Application.Services;
using System.Security.Cryptography;

namespace ShiftMate.Application.Users.Commands.ForgotPassword;

// Handlern för "Glömt lösenord".
// Den validerar e-postadressen, genererar en säker token, sparar en hash av token i databasen och skickar ett återställningsmail till användaren.
public class ForgotPasswordCommandHandler : IRequestHandler<ForgotPasswordCommand>
{
    private readonly IAppDbContext _context;
    private readonly IEmailService _emailService;
    private readonly ILogger<ForgotPasswordCommandHandler> _logger;
    private readonly IValidator<ForgotPasswordCommand> _validator;

    public ForgotPasswordCommandHandler(
        IAppDbContext context,
        IEmailService emailService,
        ILogger<ForgotPasswordCommandHandler> logger,
        IValidator<ForgotPasswordCommand> validator)
    {
        _context = context;
        _emailService = emailService;
        _logger = logger;
        _validator = validator;
    }

    public async Task Handle(ForgotPasswordCommand request, CancellationToken cancellationToken)
    {
        // 1. VALIDERING
        var validationResult = await _validator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            throw new ValidationException(validationResult.Errors);
        }

        // 2. Hitta användaren (skiftlägesokänsligt)
        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.Email == request.Email.ToLowerInvariant(), cancellationToken);

        // Anti-enumeration: returnera tyst om användaren inte finns
        if (user == null) return;

        // 3. Generera säker token (64 bytes = 512 bits)
        var tokenBytes = RandomNumberGenerator.GetBytes(64);
        var token = Convert.ToBase64String(tokenBytes)
            .Replace("+", "-")
            .Replace("/", "_")
            .TrimEnd('=');

        // 4. Spara BCrypt-hash av token + utgångstid (1 timme)
        user.ResetTokenHash = BCrypt.Net.BCrypt.HashPassword(token);
        user.ResetTokenExpiresAt = DateTime.UtcNow.AddHours(1);
        await _context.SaveChangesAsync(cancellationToken);

        // 5. Skicka e-post med återställningslänk
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
