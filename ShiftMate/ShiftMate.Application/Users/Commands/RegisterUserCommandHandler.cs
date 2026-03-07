using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using ShiftMate.Application.DTOs;
using ShiftMate.Application.Interfaces;
using ShiftMate.Application.Services;
using ShiftMate.Domain;
using System.Security.Cryptography;

namespace ShiftMate.Application.Users.Commands
{
    public class RegisterUserCommandHandler : IRequestHandler<RegisterUserCommand, UserDto>
    {
        private readonly IAppDbContext _context;
        private readonly IValidator<RegisterUserCommand> _validator;
        private readonly IEmailService _emailService;
        private readonly ILogger<RegisterUserCommandHandler> _logger;

        public RegisterUserCommandHandler(
            IAppDbContext context,
            IValidator<RegisterUserCommand> validator,
            IEmailService emailService,
            ILogger<RegisterUserCommandHandler> logger)
        {
            _context = context;
            _validator = validator;
            _emailService = emailService;
            _logger = logger;
        }

        public async Task<UserDto> Handle(RegisterUserCommand request, CancellationToken cancellationToken)
        {
            // 1. VALIDERING
            var validationResult = await _validator.ValidateAsync(request, cancellationToken);
            if (!validationResult.IsValid)
            {
                throw new ValidationException(validationResult.Errors);
            }

            // 2. Hitta organisation via inbjudningskod
            var code = request.InviteCode?.Trim().ToUpperInvariant() ?? "";
            var organization = await _context.Organizations
                .FirstOrDefaultAsync(o => o.InviteCode == code, cancellationToken);

            if (organization == null)
            {
                throw new Exception("Ogiltig inbjudningskod.");
            }

            // 3. Normalisera och kontrollera om användaren redan finns
            var email = request.Email.ToLowerInvariant();
            var existingUser = await _context.Users.FirstOrDefaultAsync(u => u.Email == email, cancellationToken);
            if (existingUser != null)
            {
                throw new Exception($"User with email '{request.Email}' already exists.");
            }

            // 4. Hasha lösenordet med BCrypt
            var passwordHash = BCrypt.Net.BCrypt.HashPassword(request.Password);

            // 5. Generera verifieringstoken
            var tokenBytes = RandomNumberGenerator.GetBytes(64);
            var token = Convert.ToBase64String(tokenBytes)
                .Replace("+", "-")
                .Replace("/", "_")
                .TrimEnd('=');

            // 6. Skapa den nya användaren
            var user = new User
            {
                FirstName = request.FirstName,
                LastName = request.LastName,
                Email = email,
                PasswordHash = passwordHash,
                Role = Role.Employee,
                OrganizationId = organization.Id,
                IsEmailVerified = false,
                EmailVerificationTokenHash = BCrypt.Net.BCrypt.HashPassword(token),
                EmailVerificationTokenExpiresAt = DateTime.UtcNow.AddHours(24)
            };

            // 7. Lägg till i databasen och spara
            _context.Users.Add(user);
            await _context.SaveChangesAsync(cancellationToken);

            // 8. Skicka verifieringsmail
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

            // 9. Returnera en DTO med den nya användarens information
            return new UserDto
            {
                Id = user.Id,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Email = user.Email,
                OrganizationId = user.OrganizationId,
                OrganizationName = organization.Name
            };
        }
    }
}
