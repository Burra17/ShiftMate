using FluentAssertions;
using ShiftMate.Application.SwapRequests.Queries;
using ShiftMate.Domain;
using ShiftMate.Tests.Support;

namespace ShiftMate.Tests;

public class GetReceivedSwapRequestsHandlerTests
{
    [Fact]
    public async Task Handle_Should_Return_Only_Requests_Where_User_Is_Target()
    {
        // Arrange
        var context = TestDbContextFactory.Create();
        var targetUser = new User
        {
            Id = Guid.NewGuid(), FirstName = "Anna", LastName = "Svensson",
            Email = "anna@test.com", PasswordHash = "hash", Role = Role.Employee
        };
        var requester = new User
        {
            Id = Guid.NewGuid(), FirstName = "Erik", LastName = "Eriksson",
            Email = "erik@test.com", PasswordHash = "hash", Role = Role.Employee
        };
        var otherUser = new User
        {
            Id = Guid.NewGuid(), FirstName = "Lisa", LastName = "Larsson",
            Email = "lisa@test.com", PasswordHash = "hash", Role = Role.Employee
        };
        context.Users.AddRange(targetUser, requester, otherUser);

        var shift = new Shift
        {
            Id = Guid.NewGuid(), UserId = requester.Id, IsUpForSwap = true,
            StartTime = DateTime.UtcNow.AddDays(1).Date.AddHours(8),
            EndTime = DateTime.UtcNow.AddDays(1).Date.AddHours(16)
        };
        var targetShift = new Shift
        {
            Id = Guid.NewGuid(), UserId = targetUser.Id, IsUpForSwap = false,
            StartTime = DateTime.UtcNow.AddDays(1).Date.AddHours(14),
            EndTime = DateTime.UtcNow.AddDays(1).Date.AddHours(22)
        };
        context.Shifts.AddRange(shift, targetShift);

        // Förfrågan riktad till targetUser — ska inkluderas
        context.SwapRequests.Add(new SwapRequest
        {
            Id = Guid.NewGuid(), ShiftId = shift.Id, RequestingUserId = requester.Id,
            TargetUserId = targetUser.Id, TargetShiftId = targetShift.Id,
            Status = SwapRequestStatus.Pending, CreatedAt = DateTime.UtcNow
        });
        // Förfrågan riktad till annan användare — ska INTE inkluderas
        context.SwapRequests.Add(new SwapRequest
        {
            Id = Guid.NewGuid(), ShiftId = shift.Id, RequestingUserId = requester.Id,
            TargetUserId = otherUser.Id, Status = SwapRequestStatus.Pending, CreatedAt = DateTime.UtcNow
        });
        await context.SaveChangesAsync(CancellationToken.None);

        var handler = new GetReceivedSwapRequestsQueryHandler(context);
        var query = new GetReceivedSwapRequestsQuery { CurrentUserId = targetUser.Id };

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().HaveCount(1);
        result[0].RequestingUser!.Email.Should().Be("erik@test.com");

        TestDbContextFactory.Destroy(context);
    }

    [Fact]
    public async Task Handle_Should_Not_Return_Non_Pending_Requests()
    {
        // Arrange
        var context = TestDbContextFactory.Create();
        var targetUser = new User
        {
            Id = Guid.NewGuid(), FirstName = "Anna", LastName = "Svensson",
            Email = "anna@test.com", PasswordHash = "hash", Role = Role.Employee
        };
        var requester = new User
        {
            Id = Guid.NewGuid(), FirstName = "Erik", LastName = "Eriksson",
            Email = "erik@test.com", PasswordHash = "hash", Role = Role.Employee
        };
        context.Users.AddRange(targetUser, requester);

        var shift = new Shift
        {
            Id = Guid.NewGuid(), UserId = requester.Id, IsUpForSwap = true,
            StartTime = DateTime.UtcNow.AddDays(1).Date.AddHours(8),
            EndTime = DateTime.UtcNow.AddDays(1).Date.AddHours(16)
        };
        context.Shifts.Add(shift);

        // Godkänd förfrågan — ska INTE inkluderas
        context.SwapRequests.Add(new SwapRequest
        {
            Id = Guid.NewGuid(), ShiftId = shift.Id, RequestingUserId = requester.Id,
            TargetUserId = targetUser.Id, Status = SwapRequestStatus.Accepted, CreatedAt = DateTime.UtcNow
        });
        await context.SaveChangesAsync(CancellationToken.None);

        var handler = new GetReceivedSwapRequestsQueryHandler(context);
        var query = new GetReceivedSwapRequestsQuery { CurrentUserId = targetUser.Id };

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().BeEmpty();

        TestDbContextFactory.Destroy(context);
    }

    [Fact]
    public async Task Handle_Should_Include_TargetShift_For_Direct_Swaps()
    {
        // Arrange
        var context = TestDbContextFactory.Create();
        var targetUser = new User
        {
            Id = Guid.NewGuid(), FirstName = "Anna", LastName = "Svensson",
            Email = "anna@test.com", PasswordHash = "hash", Role = Role.Employee
        };
        var requester = new User
        {
            Id = Guid.NewGuid(), FirstName = "Erik", LastName = "Eriksson",
            Email = "erik@test.com", PasswordHash = "hash", Role = Role.Employee
        };
        context.Users.AddRange(targetUser, requester);

        var shift = new Shift
        {
            Id = Guid.NewGuid(), UserId = requester.Id, IsUpForSwap = false,
            StartTime = DateTime.UtcNow.AddDays(1).Date.AddHours(8),
            EndTime = DateTime.UtcNow.AddDays(1).Date.AddHours(16)
        };
        var targetShift = new Shift
        {
            Id = Guid.NewGuid(), UserId = targetUser.Id, IsUpForSwap = false,
            StartTime = DateTime.UtcNow.AddDays(2).Date.AddHours(8),
            EndTime = DateTime.UtcNow.AddDays(2).Date.AddHours(16)
        };
        context.Shifts.AddRange(shift, targetShift);

        context.SwapRequests.Add(new SwapRequest
        {
            Id = Guid.NewGuid(), ShiftId = shift.Id, RequestingUserId = requester.Id,
            TargetUserId = targetUser.Id, TargetShiftId = targetShift.Id,
            Status = SwapRequestStatus.Pending, CreatedAt = DateTime.UtcNow
        });
        await context.SaveChangesAsync(CancellationToken.None);

        var handler = new GetReceivedSwapRequestsQueryHandler(context);
        var query = new GetReceivedSwapRequestsQuery { CurrentUserId = targetUser.Id };

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert — TargetShift ska vara inkluderat för direktbyten
        result.Should().HaveCount(1);
        result[0].TargetShift.Should().NotBeNull();
        result[0].TargetShift!.Id.Should().Be(targetShift.Id);

        TestDbContextFactory.Destroy(context);
    }

    [Fact]
    public async Task Handle_Should_Return_Results_Ordered_By_CreatedAt_Descending()
    {
        // Arrange
        var context = TestDbContextFactory.Create();
        var targetUser = new User
        {
            Id = Guid.NewGuid(), FirstName = "Anna", LastName = "Svensson",
            Email = "anna@test.com", PasswordHash = "hash", Role = Role.Employee
        };
        var requester = new User
        {
            Id = Guid.NewGuid(), FirstName = "Erik", LastName = "Eriksson",
            Email = "erik@test.com", PasswordHash = "hash", Role = Role.Employee
        };
        context.Users.AddRange(targetUser, requester);

        var shift1 = new Shift
        {
            Id = Guid.NewGuid(), UserId = requester.Id, IsUpForSwap = true,
            StartTime = DateTime.UtcNow.AddDays(1).Date.AddHours(8),
            EndTime = DateTime.UtcNow.AddDays(1).Date.AddHours(16)
        };
        var shift2 = new Shift
        {
            Id = Guid.NewGuid(), UserId = requester.Id, IsUpForSwap = true,
            StartTime = DateTime.UtcNow.AddDays(2).Date.AddHours(8),
            EndTime = DateTime.UtcNow.AddDays(2).Date.AddHours(16)
        };
        context.Shifts.AddRange(shift1, shift2);

        // Äldre förfrågan
        context.SwapRequests.Add(new SwapRequest
        {
            Id = Guid.NewGuid(), ShiftId = shift1.Id, RequestingUserId = requester.Id,
            TargetUserId = targetUser.Id, Status = SwapRequestStatus.Pending,
            CreatedAt = DateTime.UtcNow.AddHours(-2)
        });
        // Nyare förfrågan
        context.SwapRequests.Add(new SwapRequest
        {
            Id = Guid.NewGuid(), ShiftId = shift2.Id, RequestingUserId = requester.Id,
            TargetUserId = targetUser.Id, Status = SwapRequestStatus.Pending,
            CreatedAt = DateTime.UtcNow
        });
        await context.SaveChangesAsync(CancellationToken.None);

        var handler = new GetReceivedSwapRequestsQueryHandler(context);
        var query = new GetReceivedSwapRequestsQuery { CurrentUserId = targetUser.Id };

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert — nyaste först
        result.Should().HaveCount(2);
        result[0].CreatedAt.Should().BeAfter(result[1].CreatedAt);

        TestDbContextFactory.Destroy(context);
    }
}
