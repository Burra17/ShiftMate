using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using ShiftMate.Application.Interfaces;
using ShiftMate.Domain.Enums;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace ShiftMate.Application.Users.Commands.Login;

// Handlern för inloggning.
// Den validerar användarens e-post och lösenord, kontrollerar kontostatus och e-postverifiering, och genererar en JWT-token med relevant information som frontend behöver.
public class LoginCommandHandler : IRequestHandler<LoginCommand, string>
{
    private readonly IAppDbContext _context;
    private readonly IConfiguration _configuration;
    private readonly IValidator<LoginCommand> _validator;

    public LoginCommandHandler(IAppDbContext context, IConfiguration configuration, IValidator<LoginCommand> validator)
    {
        _context = context;
        _configuration = configuration;
        _validator = validator;
    }

    public async Task<string> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        // 1. VALIDERING
        var validationResult = await _validator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            throw new ValidationException(validationResult.Errors);
        }

        // A. Hitta användaren (skiftlägesokänsligt) med organisation
        var user = await _context.Users
            .Include(u => u.Organization)
            .FirstOrDefaultAsync(u => u.Email == request.Email.ToLowerInvariant(), cancellationToken);

        // B. Validera lösenord med BCrypt
        if (user == null || !BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
        {
            throw new Exception("Fel e-post eller lösenord.");
        }

        // B2. Kontrollera om kontot är inaktiverat
        if (!user.IsActive)
        {
            throw new Exception("Ditt konto har inaktiverats. Kontakta din chef för mer information.");
        }

        // C. Kontrollera e-postverifiering (SuperAdmin undantas)
        if (!user.IsEmailVerified && user.Role != Role.SuperAdmin)
        {
            throw new InvalidOperationException("E-postadressen är inte verifierad. Kontrollera din inkorg för verifieringslänken.");
        }

        // C. Skapa Token (Nyckeln) 🔑
        // Här lägger vi in informationen som frontend behöver!
        var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Role, user.Role.ToString()),
                new Claim("FirstName", user.FirstName),
                new Claim("LastName", user.LastName)
            };

        // SuperAdmin har ingen organisation
        if (user.OrganizationId.HasValue)
        {
            claims.Add(new Claim("OrganizationId", user.OrganizationId.Value.ToString()));
            claims.Add(new Claim("OrganizationName", user.Organization?.Name ?? ""));
        }

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
