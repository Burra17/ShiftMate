using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using ShiftMate.Domain.Entities;
using ShiftMate.Domain.Enums;
using ShiftMate.Infrastructure.Services;

namespace ShiftMate.Tests;

public class JwtTokenGeneratorTests
{
    private const string Key = "ThisIsATestSecretKeyThatIsLongEnoughForHmacSha256!!";
    private const string Issuer = "ShiftMate.Tests";
    private const string Audience = "ShiftMate.Tests";

    [Fact]
    public void GenerateToken_Should_Include_User_And_Organization_Claims_When_User_Has_Organization()
    {
        // Arrange
        var organization = new Organization { Id = Guid.NewGuid(), Name = "Testföretaget" };
        var user = CreateUser(Role.Manager, organization);

        // Act
        var token = CreateGenerator().GenerateToken(user);

        // Assert
        var claims = new JwtSecurityTokenHandler().ReadJwtToken(token).Claims.ToList();
        claims.Should().Contain(c => c.Type == ClaimTypes.NameIdentifier && c.Value == user.Id.ToString());
        claims.Should().Contain(c => c.Type == ClaimTypes.Email && c.Value == user.Email);
        claims.Should().Contain(c => c.Type == ClaimTypes.Role && c.Value == "Manager");
        claims.Should().Contain(c => c.Type == "FirstName" && c.Value == "Anna");
        claims.Should().Contain(c => c.Type == "LastName" && c.Value == "Andersson");
        claims.Should().Contain(c => c.Type == "OrganizationId" && c.Value == organization.Id.ToString());
        claims.Should().Contain(c => c.Type == "OrganizationName" && c.Value == "Testföretaget");
    }

    [Fact]
    public void GenerateToken_Should_Not_Include_Organization_Claims_When_User_Has_No_Organization()
    {
        // Arrange – SuperAdmin tillhör ingen organisation
        var user = CreateUser(Role.SuperAdmin, organization: null);

        // Act
        var token = CreateGenerator().GenerateToken(user);

        // Assert
        var claims = new JwtSecurityTokenHandler().ReadJwtToken(token).Claims.ToList();
        claims.Should().Contain(c => c.Type == ClaimTypes.Role && c.Value == "SuperAdmin");
        claims.Should().NotContain(c => c.Type == "OrganizationId");
        claims.Should().NotContain(c => c.Type == "OrganizationName");
    }

    [Fact]
    public void GenerateToken_Should_Create_Token_That_Validates_And_Expires_In_One_Hour()
    {
        // Arrange
        var user = CreateUser(Role.Employee, new Organization { Id = Guid.NewGuid(), Name = "Org" });
        var parameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = Issuer,
            ValidateAudience = true,
            ValidAudience = Audience,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Key))
        };

        // Act
        var token = CreateGenerator().GenerateToken(user);

        // Assert – samma kontroller som API:ets JwtBearer gör; kastar om något inte stämmer
        new JwtSecurityTokenHandler().ValidateToken(token, parameters, out var validated);
        validated.ValidTo.Should().BeCloseTo(DateTime.UtcNow.AddHours(1), TimeSpan.FromMinutes(1));
    }

    private static JwtTokenGenerator CreateGenerator()
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                { "Jwt:Key", Key },
                { "Jwt:Issuer", Issuer },
                { "Jwt:Audience", Audience }
            })
            .Build();
        return new JwtTokenGenerator(configuration);
    }

    private static User CreateUser(Role role, Organization? organization) => new()
    {
        Id = Guid.NewGuid(),
        Email = "anna@test.se",
        FirstName = "Anna",
        LastName = "Andersson",
        Role = role,
        OrganizationId = organization?.Id,
        Organization = organization
    };
}
