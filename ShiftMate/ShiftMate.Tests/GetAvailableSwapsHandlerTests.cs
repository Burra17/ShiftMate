using FluentAssertions;
using ShiftMate.Application.SwapRequests.Queries;
using ShiftMate.Domain;
using ShiftMate.Tests.Support;

namespace ShiftMate.Tests;

public class GetAvailableSwapsHandlerTests
{
    [Fact]
    public async Task Handle_Should_Return_Only_Pending_Swap_Requests()
    {
        // Arrange
        var context = TestDbContextFactory.Create();
        var user = new User
        {
            Id = Guid.NewGuid(), FirstName = "Anna", LastName = "Svensson",
            Email = "anna@test.com", PasswordHash = "hash", Role = Role.Employee
        };
        context.Users.Add(user);

        var shift = new Shift
        {
            Id = Guid.NewGuid(), UserId = user.Id, IsUpForSwap = true,
            StartTime = DateTime.UtcNow.AddDays(1).Date.AddHours(8),
            EndTime = DateTime.UtcNow.AddDays(1).Date.AddHours(16)
        };
        context.Shifts.Add(shift);

        // Pending — ska inkluderas
        context.SwapRequests.Add(new SwapRequest
        {
            Id = Guid.NewGuid(), ShiftId = shift.Id, RequestingUserId = user.Id,
            Status = "Pending", CreatedAt = DateTime.UtcNow
        });
        // Approved — ska INTE inkluderas
        context.SwapRequests.Add(new SwapRequest
        {
            Id = Guid.NewGuid(), ShiftId = shift.Id, RequestingUserId = user.Id,
            Status = "Approved", CreatedAt = DateTime.UtcNow
        });
        // Rejected — ska INTE inkluderas
        context.SwapRequests.Add(new SwapRequest
        {
            Id = Guid.NewGuid(), ShiftId = shift.Id, RequestingUserId = user.Id,
            Status = "Rejected", CreatedAt = DateTime.UtcNow
        });
        await context.SaveChangesAsync(CancellationToken.None);

        var handler = new GetAvailableSwapsHandler(context);

        // Act
        var result = await handler.Handle(new GetAvailableSwapsQuery(), CancellationToken.None);

        // Assert
        result.Should().HaveCount(1);
        result[0].Status.Should().Be("Pending");

        TestDbContextFactory.Destroy(context);
    }

    [Fact]
    public async Task Handle_Should_Map_Shift_And_User_To_Dto()
    {
        // Arrange
        var context = TestDbContextFactory.Create();
        var user = new User
        {
            Id = Guid.NewGuid(), FirstName = "Anna", LastName = "Svensson",
            Email = "anna@test.com", PasswordHash = "hash", Role = Role.Employee
        };
        context.Users.Add(user);

        var shift = new Shift
        {
            Id = Guid.NewGuid(), UserId = user.Id, IsUpForSwap = true,
            StartTime = DateTime.UtcNow.AddDays(1).Date.AddHours(8),
            EndTime = DateTime.UtcNow.AddDays(1).Date.AddHours(16)
        };
        context.Shifts.Add(shift);

        context.SwapRequests.Add(new SwapRequest
        {
            Id = Guid.NewGuid(), ShiftId = shift.Id, RequestingUserId = user.Id,
            Status = "Pending", CreatedAt = DateTime.UtcNow
        });
        await context.SaveChangesAsync(CancellationToken.None);

        var handler = new GetAvailableSwapsHandler(context);

        // Act
        var result = await handler.Handle(new GetAvailableSwapsQuery(), CancellationToken.None);

        // Assert — DTO ska ha Shift och RequestingUser korrekt mappade
        result.Should().HaveCount(1);
        result[0].Shift.Should().NotBeNull();
        result[0].Shift!.Id.Should().Be(shift.Id);
        result[0].RequestingUser.Should().NotBeNull();
        result[0].RequestingUser!.Email.Should().Be("anna@test.com");

        TestDbContextFactory.Destroy(context);
    }

    [Fact]
    public async Task Handle_Should_Return_Empty_List_When_No_Pending_Requests()
    {
        // Arrange
        var context = TestDbContextFactory.Create();
        var handler = new GetAvailableSwapsHandler(context);

        // Act
        var result = await handler.Handle(new GetAvailableSwapsQuery(), CancellationToken.None);

        // Assert
        result.Should().BeEmpty();

        TestDbContextFactory.Destroy(context);
    }
}
