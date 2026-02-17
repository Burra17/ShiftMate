using FluentAssertions;
using ShiftMate.Application.Shifts.Commands;
using ShiftMate.Domain;
using ShiftMate.Tests.Support;

namespace ShiftMate.Tests;

public class CancelShiftSwapCommandHandlerTests
{
    [Fact]
    public async Task Handle_Should_Throw_When_Shift_Not_Found()
    {
        // Arrange
        var context = TestDbContextFactory.Create();
        var handler = new CancelShiftSwapCommandHandler(context);

        var command = new CancelShiftSwapCommand
        {
            ShiftId = Guid.NewGuid(),
            UserId = Guid.NewGuid()
        };

        // Act & Assert
        await FluentActions.Invoking(() => handler.Handle(command, CancellationToken.None))
            .Should().ThrowAsync<Exception>()
            .WithMessage("Arbetspasset kunde inte hittas.");

        TestDbContextFactory.Destroy(context);
    }

    [Fact]
    public async Task Handle_Should_Throw_When_User_Does_Not_Own_Shift()
    {
        // Arrange - användaren försöker ångra någon annans pass
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
            Id = shiftId, UserId = ownerId, IsUpForSwap = true,
            StartTime = DateTime.UtcNow.AddDays(1).Date.AddHours(8),
            EndTime = DateTime.UtcNow.AddDays(1).Date.AddHours(16)
        });
        await context.SaveChangesAsync(CancellationToken.None);

        var handler = new CancelShiftSwapCommandHandler(context);
        var command = new CancelShiftSwapCommand
        {
            ShiftId = shiftId,
            UserId = Guid.NewGuid() // Annan användare
        };

        // Act & Assert
        await FluentActions.Invoking(() => handler.Handle(command, CancellationToken.None))
            .Should().ThrowAsync<Exception>()
            .WithMessage("Du kan inte ångra ett pass som inte är ditt.");

        TestDbContextFactory.Destroy(context);
    }

    [Fact]
    public async Task Handle_Should_Throw_When_Shift_Not_Marked_For_Swap()
    {
        // Arrange - passet är inte markerat som ledigt för byte
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

        var handler = new CancelShiftSwapCommandHandler(context);
        var command = new CancelShiftSwapCommand { ShiftId = shiftId, UserId = userId };

        // Act & Assert
        await FluentActions.Invoking(() => handler.Handle(command, CancellationToken.None))
            .Should().ThrowAsync<Exception>()
            .WithMessage("Detta pass är inte markerat som ledigt för byte.");

        TestDbContextFactory.Destroy(context);
    }

    [Fact]
    public async Task Handle_Should_Cancel_Swap_Successfully()
    {
        // Arrange - användaren ångrar sitt eget pass som är markerat för byte
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
            Id = shiftId, UserId = userId, IsUpForSwap = true,
            StartTime = DateTime.UtcNow.AddDays(1).Date.AddHours(8),
            EndTime = DateTime.UtcNow.AddDays(1).Date.AddHours(16)
        });
        await context.SaveChangesAsync(CancellationToken.None);

        var handler = new CancelShiftSwapCommandHandler(context);
        var command = new CancelShiftSwapCommand { ShiftId = shiftId, UserId = userId };

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().BeTrue();
        var updatedShift = context.Shifts.First(s => s.Id == shiftId);
        updatedShift.IsUpForSwap.Should().BeFalse();

        TestDbContextFactory.Destroy(context);
    }
}
