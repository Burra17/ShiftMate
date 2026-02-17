using FluentAssertions;
using Microsoft.Extensions.Configuration;
using ShiftMate.Application.Users.Commands;
using ShiftMate.Domain;
using ShiftMate.Tests.Support;

namespace ShiftMate.Tests;

public class LoginHandlerTests
{
    [Fact]
    public async Task Handle_Should_Throw_When_User_Not_Found()
    {
        // Arrange
        var context = TestDbContextFactory.Create();
        var handler = new LoginHandler(context, CreateConfiguration());

        var command = new LoginCommand { Email = "nonexistent@test.com", Password = "password123" };

        // Act & Assert
        await FluentActions.Invoking(() => handler.Handle(command, CancellationToken.None))
            .Should().ThrowAsync<Exception>()
            .WithMessage("Fel e-post eller lösenord.");

        TestDbContextFactory.Destroy(context);
    }

    [Fact]
    public async Task Handle_Should_Throw_When_Password_Is_Wrong()
    {
        // Arrange - användaren finns men lösenordet stämmer inte
        var context = TestDbContextFactory.Create();
        context.Users.Add(new User
        {
            Id = Guid.NewGuid(), FirstName = "Test", LastName = "Testsson",
            Email = "test@test.com",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("correctpassword"),
            Role = Role.Employee
        });
        await context.SaveChangesAsync(CancellationToken.None);

        var handler = new LoginHandler(context, CreateConfiguration());
        var command = new LoginCommand { Email = "test@test.com", Password = "wrongpassword" };

        // Act & Assert
        await FluentActions.Invoking(() => handler.Handle(command, CancellationToken.None))
            .Should().ThrowAsync<Exception>()
            .WithMessage("Fel e-post eller lösenord.");

        TestDbContextFactory.Destroy(context);
    }

    [Fact]
    public async Task Handle_Should_Return_Jwt_Token_On_Success()
    {
        // Arrange - lyckad inloggning
        var context = TestDbContextFactory.Create();
        context.Users.Add(new User
        {
            Id = Guid.NewGuid(), FirstName = "Test", LastName = "Testsson",
            Email = "test@test.com",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("password123"),
            Role = Role.Employee
        });
        await context.SaveChangesAsync(CancellationToken.None);

        var handler = new LoginHandler(context, CreateConfiguration());
        var command = new LoginCommand { Email = "test@test.com", Password = "password123" };

        // Act
        var token = await handler.Handle(command, CancellationToken.None);

        // Assert - JWT-token ska returneras (tre delar separerade med punkt)
        token.Should().NotBeNullOrEmpty();
        token.Split('.').Should().HaveCount(3);

        TestDbContextFactory.Destroy(context);
    }

    [Fact]
    public async Task Handle_Should_Match_Email_Case_Insensitively()
    {
        // Arrange - e-post med olika casing ska fungera
        var context = TestDbContextFactory.Create();
        context.Users.Add(new User
        {
            Id = Guid.NewGuid(), FirstName = "Test", LastName = "Testsson",
            Email = "test@test.com",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("password123"),
            Role = Role.Employee
        });
        await context.SaveChangesAsync(CancellationToken.None);

        var handler = new LoginHandler(context, CreateConfiguration());
        var command = new LoginCommand { Email = "Test@TEST.com", Password = "password123" };

        // Act
        var token = await handler.Handle(command, CancellationToken.None);

        // Assert
        token.Should().NotBeNullOrEmpty();

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
}
