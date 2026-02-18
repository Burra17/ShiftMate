using FluentAssertions;
using ShiftMate.Application.Shifts.Queries;
using ShiftMate.Domain;
using ShiftMate.Tests.Support;

namespace ShiftMate.Tests;

public class GetAllShiftsHandlerTests
{
    [Fact]
    public async Task Handle_Should_Return_All_Shifts_Sorted_By_StartTime()
    {
        // Arrange
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
            StartTime = DateTime.UtcNow.AddDays(2).Date.AddHours(8),
            EndTime = DateTime.UtcNow.AddDays(2).Date.AddHours(16)
        });
        context.Shifts.Add(new Shift
        {
            Id = Guid.NewGuid(), UserId = null, IsUpForSwap = false,
            StartTime = DateTime.UtcNow.AddDays(1).Date.AddHours(8),
            EndTime = DateTime.UtcNow.AddDays(1).Date.AddHours(16)
        });
        await context.SaveChangesAsync(CancellationToken.None);

        var handler = new GetAllShiftsHandler(context);

        // Act
        var result = await handler.Handle(new GetAllShiftsQuery(), CancellationToken.None);

        // Assert — returnerar alla pass sorterade efter starttid
        result.Should().HaveCount(2);
        result[0].StartTime.Should().BeBefore(result[1].StartTime);

        TestDbContextFactory.Destroy(context);
    }

    [Fact]
    public async Task Handle_Should_Filter_Only_With_Users_When_Flag_Is_True()
    {
        // Arrange
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
        context.Shifts.Add(new Shift
        {
            Id = Guid.NewGuid(), UserId = null, IsUpForSwap = false,
            StartTime = DateTime.UtcNow.AddDays(2).Date.AddHours(8),
            EndTime = DateTime.UtcNow.AddDays(2).Date.AddHours(16)
        });
        await context.SaveChangesAsync(CancellationToken.None);

        var handler = new GetAllShiftsHandler(context);

        // Act — OnlyWithUsers = true ska exkludera pass utan ägare
        var result = await handler.Handle(new GetAllShiftsQuery(OnlyWithUsers: true), CancellationToken.None);

        // Assert
        result.Should().HaveCount(1);
        result[0].UserId.Should().Be(user.Id);

        TestDbContextFactory.Destroy(context);
    }

    [Fact]
    public async Task Handle_Should_Map_User_To_Dto_When_User_Exists()
    {
        // Arrange
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

        var handler = new GetAllShiftsHandler(context);

        // Act
        var result = await handler.Handle(new GetAllShiftsQuery(), CancellationToken.None);

        // Assert — UserDto ska mappas korrekt
        result.Should().HaveCount(1);
        result[0].User.Should().NotBeNull();
        result[0].User!.FirstName.Should().Be("Anna");
        result[0].User!.Email.Should().Be("anna@test.com");
        result[0].IsUpForSwap.Should().BeTrue();

        TestDbContextFactory.Destroy(context);
    }

    [Fact]
    public async Task Handle_Should_Return_Empty_List_When_No_Shifts()
    {
        // Arrange
        var context = TestDbContextFactory.Create();
        var handler = new GetAllShiftsHandler(context);

        // Act
        var result = await handler.Handle(new GetAllShiftsQuery(), CancellationToken.None);

        // Assert
        result.Should().BeEmpty();

        TestDbContextFactory.Destroy(context);
    }
}
