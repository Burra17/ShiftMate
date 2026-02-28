using FluentAssertions;
using Microsoft.Extensions.Configuration;
using ShiftMate.Application.Users.Commands;
using ShiftMate.Domain;
using ShiftMate.Tests.Support;

namespace ShiftMate.Tests;

public class LoginHandlerTests
{
    private static readonly Guid OrgId = Guid.NewGuid();

    [Fact]
    public async Task Handle_Should_Throw_When_User_Not_Found()
    {
        var context = TestDbContextFactory.Create();
        var handler = new LoginHandler(context, CreateConfiguration());

        var command = new LoginCommand { Email = "nonexistent@test.com", Password = "password123" };

        await FluentActions.Invoking(() => handler.Handle(command, CancellationToken.None))
            .Should().ThrowAsync<Exception>()
            .WithMessage("Fel e-post eller lösenord.");

        TestDbContextFactory.Destroy(context);
    }

    [Fact]
    public async Task Handle_Should_Throw_When_Password_Is_Wrong()
    {
        var context = TestDbContextFactory.Create();
        SeedOrg(context);
        context.Users.Add(new User
        {
            Id = Guid.NewGuid(), FirstName = "Test", LastName = "Testsson",
            Email = "test@test.com",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("correctpassword"),
            Role = Role.Employee, OrganizationId = OrgId
        });
        await context.SaveChangesAsync(CancellationToken.None);

        var handler = new LoginHandler(context, CreateConfiguration());
        var command = new LoginCommand { Email = "test@test.com", Password = "wrongpassword" };

        await FluentActions.Invoking(() => handler.Handle(command, CancellationToken.None))
            .Should().ThrowAsync<Exception>()
            .WithMessage("Fel e-post eller lösenord.");

        TestDbContextFactory.Destroy(context);
    }

    [Fact]
    public async Task Handle_Should_Return_Jwt_Token_On_Success()
    {
        var context = TestDbContextFactory.Create();
        SeedOrg(context);
        context.Users.Add(new User
        {
            Id = Guid.NewGuid(), FirstName = "Test", LastName = "Testsson",
            Email = "test@test.com",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("password123"),
            Role = Role.Employee, OrganizationId = OrgId
        });
        await context.SaveChangesAsync(CancellationToken.None);

        var handler = new LoginHandler(context, CreateConfiguration());
        var command = new LoginCommand { Email = "test@test.com", Password = "password123" };

        var token = await handler.Handle(command, CancellationToken.None);

        token.Should().NotBeNullOrEmpty();
        token.Split('.').Should().HaveCount(3);

        TestDbContextFactory.Destroy(context);
    }

    [Fact]
    public async Task Handle_Should_Match_Email_Case_Insensitively()
    {
        var context = TestDbContextFactory.Create();
        SeedOrg(context);
        context.Users.Add(new User
        {
            Id = Guid.NewGuid(), FirstName = "Test", LastName = "Testsson",
            Email = "test@test.com",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("password123"),
            Role = Role.Employee, OrganizationId = OrgId
        });
        await context.SaveChangesAsync(CancellationToken.None);

        var handler = new LoginHandler(context, CreateConfiguration());
        var command = new LoginCommand { Email = "Test@TEST.com", Password = "password123" };

        var token = await handler.Handle(command, CancellationToken.None);

        token.Should().NotBeNullOrEmpty();

        TestDbContextFactory.Destroy(context);
    }

    [Fact]
    public async Task Handle_Should_Include_OrganizationId_In_Token_Claims()
    {
        var context = TestDbContextFactory.Create();
        SeedOrg(context);
        var userId = Guid.NewGuid();
        context.Users.Add(new User
        {
            Id = userId, FirstName = "Test", LastName = "Testsson",
            Email = "test@test.com",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("password123"),
            Role = Role.Employee, OrganizationId = OrgId
        });
        await context.SaveChangesAsync(CancellationToken.None);

        var handler = new LoginHandler(context, CreateConfiguration());
        var command = new LoginCommand { Email = "test@test.com", Password = "password123" };

        var token = await handler.Handle(command, CancellationToken.None);

        var jwtHandler = new System.IdentityModel.Tokens.Jwt.JwtSecurityTokenHandler();
        var jwt = jwtHandler.ReadJwtToken(token);
        jwt.Claims.Should().Contain(c => c.Type == "OrganizationId" && c.Value == OrgId.ToString());
        jwt.Claims.Should().Contain(c => c.Type == "OrganizationName" && c.Value == "Test Org");

        TestDbContextFactory.Destroy(context);
    }

    [Fact]
    public async Task Handle_Should_Not_Include_OrganizationId_For_SuperAdmin()
    {
        var context = TestDbContextFactory.Create();
        context.Users.Add(new User
        {
            Id = Guid.NewGuid(), FirstName = "Super", LastName = "Admin",
            Email = "superadmin@test.com",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("SuperPass123"),
            Role = Role.SuperAdmin, OrganizationId = null
        });
        await context.SaveChangesAsync(CancellationToken.None);

        var handler = new LoginHandler(context, CreateConfiguration());
        var command = new LoginCommand { Email = "superadmin@test.com", Password = "SuperPass123" };

        var token = await handler.Handle(command, CancellationToken.None);

        var jwtHandler = new System.IdentityModel.Tokens.Jwt.JwtSecurityTokenHandler();
        var jwt = jwtHandler.ReadJwtToken(token);
        jwt.Claims.Should().NotContain(c => c.Type == "OrganizationId");
        jwt.Claims.Should().Contain(c => c.Type == System.Security.Claims.ClaimTypes.Role && c.Value == "SuperAdmin");

        TestDbContextFactory.Destroy(context);
    }

    private static IConfiguration CreateConfiguration()
    {
        var inMemorySettings = new Dictionary<string, string?>
        {
            { "Jwt:Key", "ThisIsATestSecretKeyThatIsLongEnoughForHmacSha256!!" },
            { "Jwt:Issuer", "ShiftMate.Tests" },
            { "Jwt:Audience", "ShiftMate.Tests" }
        };
        return new ConfigurationBuilder()
            .AddInMemoryCollection(inMemorySettings)
            .Build();
    }

    private static void SeedOrg(Infrastructure.AppDbContext context)
    {
        context.Organizations.Add(new Organization { Id = OrgId, Name = "Test Org" });
        context.SaveChanges();
    }
}
