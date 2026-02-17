using FluentAssertions;
using ShiftMate.Application.SwapRequests.Commands;
using ShiftMate.Domain;
using ShiftMate.Tests.Support;

namespace ShiftMate.Tests;

public class InitiateSwapHandlerTests
{
    [Fact]
    public async Task Handle_Should_Throw_When_Shift_Not_Found()
    {
        // Arrange
        var context = TestDbContextFactory.Create();
        var handler = new InitiateSwapHandler(context);

        var command = new InitiateSwapCommand
        {
            ShiftId = Guid.NewGuid(),
            RequestingUserId = Guid.NewGuid()
        };

        // Act & Assert
        await FluentActions.Invoking(() => handler.Handle(command, CancellationToken.None))
            .Should().ThrowAsync<Exception>()
            .WithMessage("Passet hittades inte.");

        TestDbContextFactory.Destroy(context);
    }

    [Fact]
    public async Task Handle_Should_Throw_When_User_Does_Not_Own_Shift()
    {
        // Arrange - användaren försöker byta bort någon annans pass
        var context = TestDbContextFactory.Create();
        var ownerId = Guid.NewGuid();
        var shiftId = Guid.NewGuid();

        context.Users.Add(new User
        {
            Id = ownerId, FirstName = "Owner", LastName = "Ownersson",
            Email = "owner@test.com", PasswordHash = "hash", Role = Role.Employee
        });
        context.Shifts.Add(new Shift
        {
            Id = shiftId, UserId = ownerId, IsUpForSwap = false,
            StartTime = DateTime.UtcNow.AddDays(1).Date.AddHours(8),
            EndTime = DateTime.UtcNow.AddDays(1).Date.AddHours(16)
        });
        await context.SaveChangesAsync(CancellationToken.None);

        var handler = new InitiateSwapHandler(context);
        var command = new InitiateSwapCommand
        {
            ShiftId = shiftId,
            RequestingUserId = Guid.NewGuid() // Annan användare
        };

        // Act & Assert
        await FluentActions.Invoking(() => handler.Handle(command, CancellationToken.None))
            .Should().ThrowAsync<Exception>()
            .WithMessage("Du kan inte byta bort någon annans pass!");

        TestDbContextFactory.Destroy(context);
    }

    [Fact]
    public async Task Handle_Should_Create_SwapRequest_And_Mark_Shift_For_Swap()
    {
        // Arrange - användaren lägger upp sitt eget pass för byte
        var context = TestDbContextFactory.Create();
        var userId = Guid.NewGuid();
        var shiftId = Guid.NewGuid();

        context.Users.Add(new User
        {
            Id = userId, FirstName = "Test", LastName = "Testsson",
            Email = "test@test.com", PasswordHash = "hash", Role = Role.Employee
        });
        context.Shifts.Add(new Shift
        {
            Id = shiftId, UserId = userId, IsUpForSwap = false,
            StartTime = DateTime.UtcNow.AddDays(1).Date.AddHours(8),
            EndTime = DateTime.UtcNow.AddDays(1).Date.AddHours(16)
        });
        await context.SaveChangesAsync(CancellationToken.None);

        var handler = new InitiateSwapHandler(context);
        var command = new InitiateSwapCommand
        {
            ShiftId = shiftId,
            RequestingUserId = userId
        };

        // Act
        var resultId = await handler.Handle(command, CancellationToken.None);

        // Assert
        resultId.Should().NotBeEmpty();

        var shift = context.Shifts.First(s => s.Id == shiftId);
        shift.IsUpForSwap.Should().BeTrue();

        context.SwapRequests.Should().HaveCount(1);
        var swapRequest = context.SwapRequests.First();
        swapRequest.ShiftId.Should().Be(shiftId);
        swapRequest.RequestingUserId.Should().Be(userId);
        swapRequest.Status.Should().Be("Pending");

        TestDbContextFactory.Destroy(context);
    }
}
