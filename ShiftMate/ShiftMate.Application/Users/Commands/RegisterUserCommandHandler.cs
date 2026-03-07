using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using ShiftMate.Application.DTOs;
using ShiftMate.Application.Interfaces;
using ShiftMate.Domain;

namespace ShiftMate.Application.Users.Commands
{
    public class RegisterUserCommandHandler : IRequestHandler<RegisterUserCommand, UserDto>
    {
        private readonly IAppDbContext _context;
        private readonly IValidator<RegisterUserCommand> _validator;

        public RegisterUserCommandHandler(IAppDbContext context, IValidator<RegisterUserCommand> validator)
        {
            _context = context;
            _validator = validator;
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

            // 5. Skapa den nya användaren
            var user = new User
            {
                FirstName = request.FirstName,
                LastName = request.LastName,
                Email = email,
                PasswordHash = passwordHash,
                Role = Role.Employee,
                OrganizationId = organization.Id
            };

            // 6. Lägg till i databasen och spara
            _context.Users.Add(user);
            await _context.SaveChangesAsync(cancellationToken);

            // 7. Returnera en DTO med den nya användarens information
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
