using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using ShiftMate.Application.Interfaces;

namespace ShiftMate.Application.Users.Commands
{
    // Command för att återställa lösenord med token från e-post
    public record ResetPasswordCommand : IRequest
    {
        public string Token { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string NewPassword { get; set; } = string.Empty;
    }

    // Handler som verifierar token och sätter nytt lösenord
    public class ResetPasswordHandler : IRequestHandler<ResetPasswordCommand>
    {
        private readonly IAppDbContext _context;
        private readonly IValidator<ResetPasswordCommand> _validator;

        public ResetPasswordHandler(IAppDbContext context, IValidator<ResetPasswordCommand> validator)
        {
            _context = context;
            _validator = validator;
        }

        public async Task Handle(ResetPasswordCommand request, CancellationToken cancellationToken)
        {
            // 1. VALIDERING
            var validationResult = await _validator.ValidateAsync(request, cancellationToken);
            if (!validationResult.IsValid)
            {
                throw new ValidationException(validationResult.Errors);
            }

            // 2. Hitta användaren
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Email == request.Email.ToLowerInvariant(), cancellationToken);

            // Samma felmeddelande för alla ogiltiga fall (säkerhet)
            const string errorMessage = "Ogiltig eller utgången återställningslänk.";

            if (user == null)
                throw new Exception(errorMessage);

            // 3. Kontrollera att det finns en aktiv token
            if (string.IsNullOrEmpty(user.ResetTokenHash))
                throw new Exception(errorMessage);

            // 4. Kontrollera utgångstid
            if (user.ResetTokenExpiresAt == null || user.ResetTokenExpiresAt < DateTime.UtcNow)
                throw new Exception(errorMessage);

            // 5. Verifiera token mot lagrad hash
            if (!BCrypt.Net.BCrypt.Verify(request.Token, user.ResetTokenHash))
                throw new Exception(errorMessage);

            // 6. Allt ok — sätt nytt lösenord och rensa token
            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.NewPassword);
            user.ResetTokenHash = null;
            user.ResetTokenExpiresAt = null;
            await _context.SaveChangesAsync(cancellationToken);
        }
    }
}
