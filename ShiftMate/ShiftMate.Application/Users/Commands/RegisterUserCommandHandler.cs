using MediatR;
using Microsoft.EntityFrameworkCore;
using ShiftMate.Application.DTOs;
using ShiftMate.Application.Interfaces;
using ShiftMate.Domain;
using System;
using System.Threading;
using System.Threading.Tasks;

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
            // 1. Normalisera och kontrollera om användaren redan finns
            var email = request.Email.ToLowerInvariant();
            var existingUser = await _context.Users.FirstOrDefaultAsync(u => u.Email == email, cancellationToken);
            if (existingUser != null)
            {
                // Kastar ett undantag om e-posten redan är registrerad.
                // Detta kommer att fångas upp och resultera i en 400 Bad Request.
                throw new Exception($"User with email '{request.Email}' already exists.");
            }

            // 2. Hasha lösenordet med BCrypt
            // BCrypt.HashPassword genererar automatiskt ett unikt salt för varje lösenord.
            var passwordHash = BCrypt.Net.BCrypt.HashPassword(request.Password);

            // 3. Skapa den nya användaren
            var user = new User
            {
                FirstName = request.FirstName,
                LastName = request.LastName,
                Email = email, // Spara e-post i gemener
                PasswordHash = passwordHash,
                Role = Role.Employee // Nya användare får standardrollen "Employee"
            };

            // 4. Lägg till i databasen och spara
            _context.Users.Add(user);
            await _context.SaveChangesAsync(cancellationToken);

            // 5. Returnera en DTO med den nya användarens information
            return new UserDto
            {
                Id = user.Id,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Email = user.Email
            };
        }
    }
}
