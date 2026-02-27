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

        public RegisterUserCommandHandler(IAppDbContext context)
        {
            _context = context;
        }

        public async Task<UserDto> Handle(RegisterUserCommand request, CancellationToken cancellationToken)
        {
            // 1. Validera att organisationen finns
            var organization = await _context.Organizations
                .FirstOrDefaultAsync(o => o.Id == request.OrganizationId, cancellationToken);

            if (organization == null)
            {
                throw new Exception("Organisationen hittades inte.");
            }

            // 2. Normalisera och kontrollera om användaren redan finns
            var email = request.Email.ToLowerInvariant();
            var existingUser = await _context.Users.FirstOrDefaultAsync(u => u.Email == email, cancellationToken);
            if (existingUser != null)
            {
                throw new Exception($"User with email '{request.Email}' already exists.");
            }

            // 3. Hasha lösenordet med BCrypt
            var passwordHash = BCrypt.Net.BCrypt.HashPassword(request.Password);

            // 4. Skapa den nya användaren
            var user = new User
            {
                FirstName = request.FirstName,
                LastName = request.LastName,
                Email = email,
                PasswordHash = passwordHash,
                Role = Role.Employee,
                OrganizationId = request.OrganizationId
            };

            // 5. Lägg till i databasen och spara
            _context.Users.Add(user);
            await _context.SaveChangesAsync(cancellationToken);

            // 6. Returnera en DTO med den nya användarens information
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
