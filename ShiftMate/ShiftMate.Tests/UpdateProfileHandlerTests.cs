using FluentAssertions;
using ShiftMate.Application.Users.Commands;
using ShiftMate.Domain;
using ShiftMate.Tests.Support;

namespace ShiftMate.Tests;

public class UpdateProfileHandlerTests
{
    [Fact]
    public async Task Handle_Should_Throw_When_User_Not_Found()
    {
        // Arrange
        var context = TestDbContextFactory.Create();
        var handler = new UpdateProfileHandler(context);

        var command = new UpdateProfileCommand
        {
            UserId = Guid.NewGuid(),
            FirstName = "New",
            LastName = "Name",
            Email = "new@test.com"
        };

        // Act & Assert
        await FluentActions.Invoking(() => handler.Handle(command, CancellationToken.None))
            .Should().ThrowAsync<Exception>()
            .WithMessage("Användaren hittades inte.");

        TestDbContextFactory.Destroy(context);
    }

    [Fact]
    public async Task Handle_Should_Update_Profile_Successfully()
    {
        // Arrange
        var context = TestDbContextFactory.Create();
        var userId = Guid.NewGuid();

        context.Users.Add(new User
        {
            Id = userId, FirstName = "Old", LastName = "Name",
            Email = "old@test.com", PasswordHash = "hash", Role = Role.Employee
        });
        await context.SaveChangesAsync(CancellationToken.None);

        var handler = new UpdateProfileHandler(context);
        var command = new UpdateProfileCommand
        {
            UserId = userId,
            FirstName = "New",
            LastName = "Newsson",
            Email = "new@test.com"
        };

        // Act
        await handler.Handle(command, CancellationToken.None);

        // Assert
        var user = context.Users.First(u => u.Id == userId);
        user.FirstName.Should().Be("New");
        user.LastName.Should().Be("Newsson");
        user.Email.Should().Be("new@test.com");

        TestDbContextFactory.Destroy(context);
    }
}
