using FluentAssertions;
using ShiftMate.Application.SwapRequests.Queries;
using ShiftMate.Domain;
using ShiftMate.Tests.Support;

namespace ShiftMate.Tests;

public class GetSentSwapRequestsHandlerTests
{
    private static readonly Guid OrgId = Guid.NewGuid();

    [Fact]
    public async Task Handle_Should_Return_Only_Requests_Sent_By_Current_User()
    {
        var context = TestDbContextFactory.Create();
        SeedOrg(context);
        var sender = new User
        {
            Id = Guid.NewGuid(), FirstName = "Anna", LastName = "Svensson",
            Email = "anna@test.com", PasswordHash = "hash", Role = Role.Employee, OrganizationId = OrgId
        };
        var target = new User
        {
            Id = Guid.NewGuid(), FirstName = "Erik", LastName = "Eriksson",
            Email = "erik@test.com", PasswordHash = "hash", Role = Role.Employee, OrganizationId = OrgId
        };
        context.Users.AddRange(sender, target);

        var shift = new Shift
        {
            Id = Guid.NewGuid(), UserId = sender.Id, IsUpForSwap = true, OrganizationId = OrgId,
            StartTime = DateTime.UtcNow.AddDays(1).Date.AddHours(8),
            EndTime = DateTime.UtcNow.AddDays(1).Date.AddHours(16)
        };
        context.Shifts.Add(shift);

        context.SwapRequests.Add(new SwapRequest
        {
            Id = Guid.NewGuid(), ShiftId = shift.Id, RequestingUserId = sender.Id,
            TargetUserId = target.Id, Status = SwapRequestStatus.Pending, CreatedAt = DateTime.UtcNow
        });
        context.SwapRequests.Add(new SwapRequest
        {
            Id = Guid.NewGuid(), ShiftId = shift.Id, RequestingUserId = target.Id,
            TargetUserId = sender.Id, Status = SwapRequestStatus.Pending, CreatedAt = DateTime.UtcNow
        });
        await context.SaveChangesAsync(CancellationToken.None);

        var handler = new GetSentSwapRequestsQueryHandler(context);
        var query = new GetSentSwapRequestsQuery { CurrentUserId = sender.Id };

        var result = await handler.Handle(query, CancellationToken.None);

        result.Should().HaveCount(1);

        TestDbContextFactory.Destroy(context);
    }

    [Fact]
    public async Task Handle_Should_Not_Return_Non_Pending_Requests()
    {
        var context = TestDbContextFactory.Create();
        SeedOrg(context);
        var sender = new User
        {
            Id = Guid.NewGuid(), FirstName = "Anna", LastName = "Svensson",
            Email = "anna@test.com", PasswordHash = "hash", Role = Role.Employee, OrganizationId = OrgId
        };
        var target = new User
        {
            Id = Guid.NewGuid(), FirstName = "Erik", LastName = "Eriksson",
            Email = "erik@test.com", PasswordHash = "hash", Role = Role.Employee, OrganizationId = OrgId
        };
        context.Users.AddRange(sender, target);

        var shift = new Shift
        {
            Id = Guid.NewGuid(), UserId = sender.Id, IsUpForSwap = true, OrganizationId = OrgId,
            StartTime = DateTime.UtcNow.AddDays(1).Date.AddHours(8),
            EndTime = DateTime.UtcNow.AddDays(1).Date.AddHours(16)
        };
        context.Shifts.Add(shift);

        context.SwapRequests.Add(new SwapRequest
        {
            Id = Guid.NewGuid(), ShiftId = shift.Id, RequestingUserId = sender.Id,
            TargetUserId = target.Id, Status = SwapRequestStatus.Accepted, CreatedAt = DateTime.UtcNow
        });
        await context.SaveChangesAsync(CancellationToken.None);

        var handler = new GetSentSwapRequestsQueryHandler(context);
        var query = new GetSentSwapRequestsQuery { CurrentUserId = sender.Id };

        var result = await handler.Handle(query, CancellationToken.None);

        result.Should().BeEmpty();

        TestDbContextFactory.Destroy(context);
    }

    [Fact]
    public async Task Handle_Should_Map_TargetUser_To_TargetUser_Dto()
    {
        var context = TestDbContextFactory.Create();
        SeedOrg(context);
        var sender = new User
        {
            Id = Guid.NewGuid(), FirstName = "Anna", LastName = "Svensson",
            Email = "anna@test.com", PasswordHash = "hash", Role = Role.Employee, OrganizationId = OrgId
        };
        var target = new User
        {
            Id = Guid.NewGuid(), FirstName = "Erik", LastName = "Eriksson",
            Email = "erik@test.com", PasswordHash = "hash", Role = Role.Employee, OrganizationId = OrgId
        };
        context.Users.AddRange(sender, target);

        var shift = new Shift
        {
            Id = Guid.NewGuid(), UserId = sender.Id, IsUpForSwap = true, OrganizationId = OrgId,
            StartTime = DateTime.UtcNow.AddDays(1).Date.AddHours(8),
            EndTime = DateTime.UtcNow.AddDays(1).Date.AddHours(16)
        };
        context.Shifts.Add(shift);

        context.SwapRequests.Add(new SwapRequest
        {
            Id = Guid.NewGuid(), ShiftId = shift.Id, RequestingUserId = sender.Id,
            TargetUserId = target.Id, Status = SwapRequestStatus.Pending, CreatedAt = DateTime.UtcNow
        });
        await context.SaveChangesAsync(CancellationToken.None);

        var handler = new GetSentSwapRequestsQueryHandler(context);
        var query = new GetSentSwapRequestsQuery { CurrentUserId = sender.Id };

        var result = await handler.Handle(query, CancellationToken.None);

        result.Should().HaveCount(1);
        result[0].TargetUser.Should().NotBeNull();
        result[0].TargetUser!.FirstName.Should().Be("Erik");
        result[0].TargetUser!.Email.Should().Be("erik@test.com");
        result[0].RequestingUser.Should().BeNull();

        TestDbContextFactory.Destroy(context);
    }

    [Fact]
    public async Task Handle_Should_Include_TargetShift_For_Direct_Swaps()
    {
        var context = TestDbContextFactory.Create();
        SeedOrg(context);
        var sender = new User
        {
            Id = Guid.NewGuid(), FirstName = "Anna", LastName = "Svensson",
            Email = "anna@test.com", PasswordHash = "hash", Role = Role.Employee, OrganizationId = OrgId
        };
        var target = new User
        {
            Id = Guid.NewGuid(), FirstName = "Erik", LastName = "Eriksson",
            Email = "erik@test.com", PasswordHash = "hash", Role = Role.Employee, OrganizationId = OrgId
        };
        context.Users.AddRange(sender, target);

        var shift = new Shift
        {
            Id = Guid.NewGuid(), UserId = sender.Id, IsUpForSwap = false, OrganizationId = OrgId,
            StartTime = DateTime.UtcNow.AddDays(1).Date.AddHours(8),
            EndTime = DateTime.UtcNow.AddDays(1).Date.AddHours(16)
        };
        var targetShift = new Shift
        {
            Id = Guid.NewGuid(), UserId = target.Id, IsUpForSwap = false, OrganizationId = OrgId,
            StartTime = DateTime.UtcNow.AddDays(2).Date.AddHours(8),
            EndTime = DateTime.UtcNow.AddDays(2).Date.AddHours(16)
        };
        context.Shifts.AddRange(shift, targetShift);

        context.SwapRequests.Add(new SwapRequest
        {
            Id = Guid.NewGuid(), ShiftId = shift.Id, RequestingUserId = sender.Id,
            TargetUserId = target.Id, TargetShiftId = targetShift.Id,
            Status = SwapRequestStatus.Pending, CreatedAt = DateTime.UtcNow
        });
        await context.SaveChangesAsync(CancellationToken.None);

        var handler = new GetSentSwapRequestsQueryHandler(context);
        var query = new GetSentSwapRequestsQuery { CurrentUserId = sender.Id };

        var result = await handler.Handle(query, CancellationToken.None);

        result.Should().HaveCount(1);
        result[0].TargetShift.Should().NotBeNull();
        result[0].TargetShift!.Id.Should().Be(targetShift.Id);

        TestDbContextFactory.Destroy(context);
    }

    private static void SeedOrg(Infrastructure.AppDbContext context)
    {
        context.Organizations.Add(new Organization { Id = OrgId, Name = "Test Org" });
        context.SaveChanges();
    }
}
