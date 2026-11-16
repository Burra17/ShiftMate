using System.IdentityModel.Tokens.Jwt;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.IdentityModel.Tokens;
using ShiftMate.Application.Interfaces;
using ShiftMate.Domain.Entities;
using ShiftMate.Domain.Enums;
using ShiftMate.Infrastructure;

namespace ShiftMate.Tests.Integration;

// Startar hela API:et i minnet för integrationstester.
// Byter ut PostgreSQL mot en InMemory-databas och e-post mot en fejk, så att testerna
// går igenom riktiga HTTP-anrop (routing, [Authorize], middleware) utan externa beroenden.
public class ShiftMateApiFactory : WebApplicationFactory<Program>
{
    private const string JwtKey = "IntegrationTestKeyThatIsLongEnoughForHmacSha256!!";
    private const string JwtIssuer = "ShiftMate.IntegrationTests";
    private const string JwtAudience = "ShiftMate.IntegrationTests";

    // Unik databas per fabrik, så att testklasser inte delar data
    private readonly string _databaseName = $"ShiftMateIntegration-{Guid.NewGuid()}";

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        // "Testing" istället för Development: hoppar över migreringar och seed i Program.cs
        builder.UseEnvironment("Testing");

        // UseSetting gäller redan när Program.cs läser konfigurationen
        builder.UseSetting("Jwt:Key", JwtKey);
        builder.UseSetting("Jwt:Issuer", JwtIssuer);
        builder.UseSetting("Jwt:Audience", JwtAudience);

        builder.ConfigureServices(services =>
        {
            services.RemoveAll<DbContextOptions<AppDbContext>>();
            services.AddDbContext<AppDbContext>(options => options.UseInMemoryDatabase(_databaseName));

            services.RemoveAll<IEmailService>();
            services.AddScoped<IEmailService, FakeEmailService>();
        });
    }

    // Skapar en organisation och en användare med given roll direkt i databasen
    public async Task<User> CreateUserAsync(Role role, string? password = null)
    {
        using var scope = Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        var organization = new Organization
        {
            Id = Guid.NewGuid(),
            Name = $"Org-{Guid.NewGuid()}",
            InviteCode = Guid.NewGuid().ToString("N")[..8].ToUpperInvariant()
        };
        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = $"{Guid.NewGuid()}@test.se",
            FirstName = "Test",
            LastName = role.ToString(),
            Role = role,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(password ?? "Password123"),
            IsEmailVerified = true,
            OrganizationId = organization.Id
        };

        context.Organizations.Add(organization);
        context.Users.Add(user);
        await context.SaveChangesAsync();
        return user;
    }

    // Skapar ett pass i användarens organisation direkt i databasen
    public async Task<Shift> CreateShiftAsync(Guid organizationId, Guid? userId = null)
    {
        using var scope = Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        var start = DateTime.UtcNow.Date.AddDays(3).AddHours(8);
        var shift = new Shift
        {
            Id = Guid.NewGuid(),
            OrganizationId = organizationId,
            UserId = userId,
            StartTime = start,
            EndTime = start.AddHours(8)
        };

        context.Shifts.Add(shift);
        await context.SaveChangesAsync();
        return shift;
    }

    // HTTP-klient med en JWT som har samma claims som LoginCommandHandler utfärdar
    public HttpClient CreateAuthenticatedClient(User user)
    {
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new(ClaimTypes.Email, user.Email),
            new(ClaimTypes.Role, user.Role.ToString()),
            new("FirstName", user.FirstName),
            new("LastName", user.LastName)
        };
        if (user.OrganizationId.HasValue)
        {
            claims.Add(new Claim("OrganizationId", user.OrganizationId.Value.ToString()));
        }

        var credentials = new SigningCredentials(
            new SymmetricSecurityKey(Encoding.UTF8.GetBytes(JwtKey)), SecurityAlgorithms.HmacSha256);
        var token = new JwtSecurityToken(JwtIssuer, JwtAudience, claims,
            expires: DateTime.UtcNow.AddHours(1), signingCredentials: credentials);

        var client = CreateClient();
        client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", new JwtSecurityTokenHandler().WriteToken(token));
        return client;
    }

    private class FakeEmailService : IEmailService
    {
        public Task SendEmailAsync(string toEmail, string subject, string message) => Task.CompletedTask;
    }
}
