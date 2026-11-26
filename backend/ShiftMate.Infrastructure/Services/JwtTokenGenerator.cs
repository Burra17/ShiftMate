using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using ShiftMate.Application.Interfaces;
using ShiftMate.Domain.Entities;

namespace ShiftMate.Infrastructure.Services;

// Den enda platsen där JWT-token skapas – används av inloggningen och av integrationstesterna,
// så att alla token har exakt samma claims.
public class JwtTokenGenerator : IJwtTokenGenerator
{
    private readonly IConfiguration _configuration;

    public JwtTokenGenerator(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public string GenerateToken(User user)
    {
        // Informationen som frontend behöver
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new(ClaimTypes.Email, user.Email),
            new(ClaimTypes.Role, user.Role.ToString()),
            new("FirstName", user.FirstName),
            new("LastName", user.LastName)
        };

        // SuperAdmin har ingen organisation
        if (user.OrganizationId.HasValue)
        {
            claims.Add(new Claim("OrganizationId", user.OrganizationId.Value.ToString()));
            claims.Add(new Claim("OrganizationName", user.Organization?.Name ?? ""));
        }

        // Demo-sandlåda – frontend visar en banner om att ändringar raderas
        if (user.Organization?.IsDemo == true)
        {
            claims.Add(new Claim("IsDemo", "true"));
        }

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]!));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _configuration["Jwt:Issuer"],
            audience: _configuration["Jwt:Audience"],
            claims: claims,
            expires: DateTime.UtcNow.AddHours(1),
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
