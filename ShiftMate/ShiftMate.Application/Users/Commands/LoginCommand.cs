using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using ShiftMate.Application.Interfaces;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace ShiftMate.Application.Users.Commands
{
    // 1. DATA
    public record LoginCommand : IRequest<string>
    {
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }

    // 2. LOGIK
    public class LoginHandler : IRequestHandler<LoginCommand, string>
    {
        private readonly IAppDbContext _context;
        private readonly IConfiguration _configuration;

        public LoginHandler(IAppDbContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        public async Task<string> Handle(LoginCommand request, CancellationToken cancellationToken)
        {
            // A. Hitta användaren (skiftlägesokänsligt) med organisation
            var user = await _context.Users
                .Include(u => u.Organization)
                .FirstOrDefaultAsync(u => u.Email == request.Email.ToLowerInvariant(), cancellationToken);

            // B. Validera lösenord med BCrypt
            if (user == null || !BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
            {
                throw new Exception("Fel e-post eller lösenord.");
            }

            // C. Skapa Token (Nyckeln) 🔑
            // Här lägger vi in informationen som frontend behöver!
            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Role, user.Role.ToString()),
                new Claim("FirstName", user.FirstName),
                new Claim("LastName", user.LastName),
                new Claim("OrganizationId", user.OrganizationId.ToString()),
                new Claim("OrganizationName", user.Organization?.Name ?? "")
            };

            // Hämta hemlig nyckel
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]!));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            // Skapa själva token-objektet
            var token = new JwtSecurityToken(
                issuer: _configuration["Jwt:Issuer"],
                audience: _configuration["Jwt:Audience"],
                claims: claims,
                expires: DateTime.Now.AddHours(1),
                signingCredentials: creds
            );

            // Returnera som textsträng
            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}