using FluentAssertions;
using ShiftMate.Application.Users.Commands;
using ShiftMate.Domain;
using ShiftMate.Tests.Support;

namespace ShiftMate.Tests;

public class RegisterUserCommandHandlerTests
{
    [Fact]
    public async Task Handle_Should_Register_User_Successfully()
    {
        // Arrange
        var context = TestDbContextFactory.Create();
        var handler = new RegisterUserCommandHandler(context);

        var command = new RegisterUserCommand("Test", "Testsson", "test@test.com", "password123");

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().NotBeEmpty();
        result.FirstName.Should().Be("Test");
        result.LastName.Should().Be("Testsson");
        result.Email.Should().Be("test@test.com");

        context.Users.Should().HaveCount(1);
        var user = context.Users.First();
        user.Role.Should().Be(Role.Employee);

        TestDbContextFactory.Destroy(context);
    }

    [Fact]
    public async Task Handle_Should_Throw_When_Email_Already_Exists()
    {
        // Arrange - e-postadressen finns redan
        var context = TestDbContextFactory.Create();
        context.Users.Add(new User
        {
            Id = Guid.NewGuid(), FirstName = "Existing", LastName = "User",
            Email = "test@test.com", PasswordHash = "hash", Role = Role.Employee
        });
        await context.SaveChangesAsync(CancellationToken.None);

        var handler = new RegisterUserCommandHandler(context);
        var command = new RegisterUserCommand("Test", "Testsson", "Test@Test.com", "password123");

        // Act & Assert - e-post jämförs case-insensitive
        await FluentActions.Invoking(() => handler.Handle(command, CancellationToken.None))
            .Should().ThrowAsync<Exception>()
            .WithMessage("*already exists*");

        TestDbContextFactory.Destroy(context);
    }

    [Fact]
    public async Task Handle_Should_Store_Email_As_Lowercase()
    {
        // Arrange
        var context = TestDbContextFactory.Create();
        var handler = new RegisterUserCommandHandler(context);

        var command = new RegisterUserCommand("Test", "Testsson", "Test@TEST.com", "password123");

        // Act
        await handler.Handle(command, CancellationToken.None);

        // Assert - e-post normaliseras till gemener
        var user = context.Users.First();
        user.Email.Should().Be("test@test.com");

        TestDbContextFactory.Destroy(context);
    }

    [Fact]
    public async Task Handle_Should_Hash_Password()
    {
        // Arrange
        var context = TestDbContextFactory.Create();
        var handler = new RegisterUserCommandHandler(context);

        var command = new RegisterUserCommand("Test", "Testsson", "test@test.com", "password123");

        // Act
        await handler.Handle(command, CancellationToken.None);

        // Assert - lösenordet ska vara hashat, inte i klartext
        var user = context.Users.First();
        user.PasswordHash.Should().NotBe("password123");
        user.PasswordHash.Should().StartWith("$2"); // BCrypt-prefix

        TestDbContextFactory.Destroy(context);
    }
}
