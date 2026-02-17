using FluentAssertions;
using FluentValidation;
using FluentValidation.Results;
using Moq;
using ShiftMate.Application.Users.Commands;
using ShiftMate.Domain;
using ShiftMate.Tests.Support;

namespace ShiftMate.Tests;

public class ChangePasswordHandlerTests
{
    [Fact]
    public async Task Handle_Should_Throw_When_User_Not_Found()
    {
        // Arrange
        var context = TestDbContextFactory.Create();
        var handler = CreateHandler(context);

        var command = new ChangePasswordCommand
        {
            UserId = Guid.NewGuid(),
            CurrentPassword = "oldpassword",
            NewPassword = "newpassword123"
        };

        // Act & Assert
        await FluentActions.Invoking(() => handler.Handle(command, CancellationToken.None))
            .Should().ThrowAsync<Exception>()
            .WithMessage("Användaren hittades inte.");

        TestDbContextFactory.Destroy(context);
    }

    [Fact]
    public async Task Handle_Should_Throw_When_Current_Password_Is_Wrong()
    {
        // Arrange - nuvarande lösenord stämmer inte
        var context = TestDbContextFactory.Create();
        var userId = Guid.NewGuid();

        context.Users.Add(new User
        {
            Id = userId, FirstName = "Test", LastName = "Testsson",
            Email = "test@test.com",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("correctpassword"),
            Role = Role.Employee
        });
        await context.SaveChangesAsync(CancellationToken.None);

        var handler = CreateHandler(context);
        var command = new ChangePasswordCommand
        {
            UserId = userId,
            CurrentPassword = "wrongpassword",
            NewPassword = "newpassword123"
        };

        // Act & Assert
        await FluentActions.Invoking(() => handler.Handle(command, CancellationToken.None))
            .Should().ThrowAsync<Exception>()
            .WithMessage("Nuvarande lösenord är felaktigt.");

        TestDbContextFactory.Destroy(context);
    }

    [Fact]
    public async Task Handle_Should_Change_Password_Successfully()
    {
        // Arrange
        var context = TestDbContextFactory.Create();
        var userId = Guid.NewGuid();
        var oldPassword = "oldpassword123";

        context.Users.Add(new User
        {
            Id = userId, FirstName = "Test", LastName = "Testsson",
            Email = "test@test.com",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(oldPassword),
            Role = Role.Employee
        });
        await context.SaveChangesAsync(CancellationToken.None);

        var handler = CreateHandler(context);
        var command = new ChangePasswordCommand
        {
            UserId = userId,
            CurrentPassword = oldPassword,
            NewPassword = "newpassword123"
        };

        // Act
        await handler.Handle(command, CancellationToken.None);

        // Assert - nya lösenordet ska verifiera korrekt
        var user = context.Users.First(u => u.Id == userId);
        BCrypt.Net.BCrypt.Verify("newpassword123", user.PasswordHash).Should().BeTrue();
        BCrypt.Net.BCrypt.Verify(oldPassword, user.PasswordHash).Should().BeFalse();

        TestDbContextFactory.Destroy(context);
    }

    private static ChangePasswordHandler CreateHandler(Infrastructure.AppDbContext context)
    {
        var validatorMock = new Mock<IValidator<ChangePasswordCommand>>();
        validatorMock.Setup(v => v.ValidateAsync(It.IsAny<ChangePasswordCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult());
        return new ChangePasswordHandler(context, validatorMock.Object);
    }
}
