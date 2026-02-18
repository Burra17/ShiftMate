using FluentAssertions;
using ShiftMate.Application.Shifts.Queries;
using ShiftMate.Domain;
using ShiftMate.Tests.Support;

namespace ShiftMate.Tests;

public class GetClaimableShiftsHandlerTests
{
    [Fact]
    public async Task Handle_Should_Return_Shifts_With_No_Owner()
    {
        // Arrange — öppet pass utan ägare ska vara claimable
        var context = TestDbContextFactory.Create();
        var user = new User
        {
            Id = Guid.NewGuid(), FirstName = "Anna", LastName = "Svensson",
            Email = "anna@test.com", PasswordHash = "hash", Role = Role.Employee
        };
        context.Users.Add(user);
        context.Shifts.Add(new Shift
        {
            Id = Guid.NewGuid(), UserId = null, IsUpForSwap = false,
            StartTime = DateTime.UtcNow.AddDays(1).Date.AddHours(8),
            EndTime = DateTime.UtcNow.AddDays(1).Date.AddHours(16)
        });
        // Pass med ägare som INTE är uppe för byte — ska INTE inkluderas
        context.Shifts.Add(new Shift
        {
            Id = Guid.NewGuid(), UserId = user.Id, IsUpForSwap = false,
            StartTime = DateTime.UtcNow.AddDays(2).Date.AddHours(8),
            EndTime = DateTime.UtcNow.AddDays(2).Date.AddHours(16)
        });
        await context.SaveChangesAsync(CancellationToken.None);

        var handler = new GetClaimableShiftsHandler(context);

        // Act
        var result = await handler.Handle(new GetClaimableShiftsQuery(), CancellationToken.None);

        // Assert
        result.Should().HaveCount(1);
        result[0].UserId.Should().BeNull();

        TestDbContextFactory.Destroy(context);
    }

    [Fact]
    public async Task Handle_Should_Return_Shifts_Marked_As_Up_For_Swap()
    {
        // Arrange — pass markerat för byte ska vara claimable
        var context = TestDbContextFactory.Create();
        var user = new User
        {
            Id = Guid.NewGuid(), FirstName = "Anna", LastName = "Svensson",
            Email = "anna@test.com", PasswordHash = "hash", Role = Role.Employee
        };
        context.Users.Add(user);
        context.Shifts.Add(new Shift
        {
            Id = Guid.NewGuid(), UserId = user.Id, IsUpForSwap = true,
            StartTime = DateTime.UtcNow.AddDays(1).Date.AddHours(8),
            EndTime = DateTime.UtcNow.AddDays(1).Date.AddHours(16)
        });
        await context.SaveChangesAsync(CancellationToken.None);

        var handler = new GetClaimableShiftsHandler(context);

        // Act
        var result = await handler.Handle(new GetClaimableShiftsQuery(), CancellationToken.None);

        // Assert
        result.Should().HaveCount(1);
        result[0].IsUpForSwap.Should().BeTrue();
        result[0].User.Should().NotBeNull();
        result[0].User!.FirstName.Should().Be("Anna");

        TestDbContextFactory.Destroy(context);
    }

    [Fact]
    public async Task Handle_Should_Not_Return_Assigned_Shifts_Not_Up_For_Swap()
    {
        // Arrange — pass med ägare och IsUpForSwap = false ska INTE vara claimable
        var context = TestDbContextFactory.Create();
        var user = new User
        {
            Id = Guid.NewGuid(), FirstName = "Anna", LastName = "Svensson",
            Email = "anna@test.com", PasswordHash = "hash", Role = Role.Employee
        };
        context.Users.Add(user);
        context.Shifts.Add(new Shift
        {
            Id = Guid.NewGuid(), UserId = user.Id, IsUpForSwap = false,
            StartTime = DateTime.UtcNow.AddDays(1).Date.AddHours(8),
            EndTime = DateTime.UtcNow.AddDays(1).Date.AddHours(16)
        });
        await context.SaveChangesAsync(CancellationToken.None);

        var handler = new GetClaimableShiftsHandler(context);

        // Act
        var result = await handler.Handle(new GetClaimableShiftsQuery(), CancellationToken.None);

        // Assert
        result.Should().BeEmpty();

        TestDbContextFactory.Destroy(context);
    }

    [Fact]
    public async Task Handle_Should_Return_Shifts_Sorted_By_StartTime()
    {
        // Arrange
        var context = TestDbContextFactory.Create();
        context.Shifts.Add(new Shift
        {
            Id = Guid.NewGuid(), UserId = null, IsUpForSwap = false,
            StartTime = DateTime.UtcNow.AddDays(3).Date.AddHours(8),
            EndTime = DateTime.UtcNow.AddDays(3).Date.AddHours(16)
        });
        context.Shifts.Add(new Shift
        {
            Id = Guid.NewGuid(), UserId = null, IsUpForSwap = false,
            StartTime = DateTime.UtcNow.AddDays(1).Date.AddHours(8),
            EndTime = DateTime.UtcNow.AddDays(1).Date.AddHours(16)
        });
        await context.SaveChangesAsync(CancellationToken.None);

        var handler = new GetClaimableShiftsHandler(context);

        // Act
        var result = await handler.Handle(new GetClaimableShiftsQuery(), CancellationToken.None);

        // Assert
        result.Should().HaveCount(2);
        result[0].StartTime.Should().BeBefore(result[1].StartTime);

        TestDbContextFactory.Destroy(context);
    }
}
