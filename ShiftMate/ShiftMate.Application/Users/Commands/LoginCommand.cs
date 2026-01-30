using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration; // För att läsa "Secret Key"
using Microsoft.IdentityModel.Tokens;     // För att skapa Token
using ShiftMate.Application.Interfaces;
using System.IdentityModel.Tokens.Jwt;    // För att hantera JWT
using System.Security.Claims;             // För att lägga info i Token (Claims)
using System.Text;

namespace ShiftMate.Application.Users.Commands
{
    // 1. DATA: Vad skickar användaren in?
    public record LoginCommand : IRequest<string>
    {
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }

    // 2. LOGIK: Validera och skapa nyckel
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
            // A. Hitta användaren
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Email == request.Email, cancellationToken);

            // B. Enkel kontroll (I framtiden kör vi riktig Hash-koll här)
            if (user == null || user.PasswordHash != request.Password)
            {
                throw new Exception("Fel e-post eller lösenord.");
            }

            // C. Skapa Token (Nyckeln)
            // Vi lägger in ID, Email och Roll i nyckeln så API:et vet vem det är.
            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Role, user.Role)
            };

            // Hämta vår hemliga kod från appsettings.json
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]!));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _configuration["Jwt:Issuer"],
                audience: _configuration["Jwt:Audience"],
                claims: claims,
                expires: DateTime.Now.AddHours(1), // Nyckeln gäller i 1 timme
                signingCredentials: creds
            );

            // Gör om objektet till en sträng (t.ex. "eyJhbGciOi...")
            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}