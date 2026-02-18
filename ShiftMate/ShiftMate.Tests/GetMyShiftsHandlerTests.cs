using FluentAssertions;
using ShiftMate.Application.Shifts.Queries;
using ShiftMate.Domain;
using ShiftMate.Tests.Support;

namespace ShiftMate.Tests;

public class GetMyShiftsHandlerTests
{
    [Fact]
    public async Task Handle_Should_Return_Only_Shifts_Belonging_To_User()
    {
        // Arrange
        var context = TestDbContextFactory.Create();
        var userId = Guid.NewGuid();
        var otherUserId = Guid.NewGuid();

        context.Users.Add(new User
        {
            Id = userId, FirstName = "Anna", LastName = "Svensson",
            Email = "anna@test.com", PasswordHash = "hash", Role = Role.Employee
        });
        context.Users.Add(new User
        {
            Id = otherUserId, FirstName = "Erik", LastName = "Eriksson",
            Email = "erik@test.com", PasswordHash = "hash", Role = Role.Employee
        });
        context.Shifts.Add(new Shift
        {
            Id = Guid.NewGuid(), UserId = userId, IsUpForSwap = false,
            StartTime = DateTime.UtcNow.AddDays(1).Date.AddHours(8),
            EndTime = DateTime.UtcNow.AddDays(1).Date.AddHours(16)
        });
        context.Shifts.Add(new Shift
        {
            Id = Guid.NewGuid(), UserId = otherUserId, IsUpForSwap = false,
            StartTime = DateTime.UtcNow.AddDays(1).Date.AddHours(8),
            EndTime = DateTime.UtcNow.AddDays(1).Date.AddHours(16)
        });
        context.Shifts.Add(new Shift
        {
            Id = Guid.NewGuid(), UserId = null, IsUpForSwap = false,
            StartTime = DateTime.UtcNow.AddDays(2).Date.AddHours(8),
            EndTime = DateTime.UtcNow.AddDays(2).Date.AddHours(16)
        });
        await context.SaveChangesAsync(CancellationToken.None);

        var handler = new GetMyShiftsHandler(context);

        // Act
        var result = await handler.Handle(new GetMyShiftsQuery(userId), CancellationToken.None);

        // Assert — ska bara returnera Annas pass
        result.Should().HaveCount(1);
        result[0].UserId.Should().Be(userId);

        TestDbContextFactory.Destroy(context);
    }

    [Fact]
    public async Task Handle_Should_Return_Shifts_Sorted_By_StartTime()
    {
        // Arrange
        var context = TestDbContextFactory.Create();
        var userId = Guid.NewGuid();

        context.Users.Add(new User
        {
            Id = userId, FirstName = "Anna", LastName = "Svensson",
            Email = "anna@test.com", PasswordHash = "hash", Role = Role.Employee
        });
        context.Shifts.Add(new Shift
        {
            Id = Guid.NewGuid(), UserId = userId, IsUpForSwap = false,
            StartTime = DateTime.UtcNow.AddDays(3).Date.AddHours(8),
            EndTime = DateTime.UtcNow.AddDays(3).Date.AddHours(16)
        });
        context.Shifts.Add(new Shift
        {
            Id = Guid.NewGuid(), UserId = userId, IsUpForSwap = false,
            StartTime = DateTime.UtcNow.AddDays(1).Date.AddHours(8),
            EndTime = DateTime.UtcNow.AddDays(1).Date.AddHours(16)
        });
        await context.SaveChangesAsync(CancellationToken.None);

        var handler = new GetMyShiftsHandler(context);

        // Act
        var result = await handler.Handle(new GetMyShiftsQuery(userId), CancellationToken.None);

        // Assert
        result.Should().HaveCount(2);
        result[0].StartTime.Should().BeBefore(result[1].StartTime);

        TestDbContextFactory.Destroy(context);
    }

    [Fact]
    public async Task Handle_Should_Return_Empty_List_When_User_Has_No_Shifts()
    {
        // Arrange
        var context = TestDbContextFactory.Create();
        var handler = new GetMyShiftsHandler(context);

        // Act
        var result = await handler.Handle(new GetMyShiftsQuery(Guid.NewGuid()), CancellationToken.None);

        // Assert
        result.Should().BeEmpty();

        TestDbContextFactory.Destroy(context);
    }
}
