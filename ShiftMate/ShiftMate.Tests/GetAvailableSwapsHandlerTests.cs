using FluentAssertions;
using ShiftMate.Application.SwapRequests.Queries;
using ShiftMate.Domain;
using ShiftMate.Tests.Support;

namespace ShiftMate.Tests;

public class GetAvailableSwapsHandlerTests
{
    private static readonly Guid OrgId = Guid.NewGuid();

    [Fact]
    public async Task Handle_Should_Return_Only_Pending_Swap_Requests()
    {
        var context = TestDbContextFactory.Create();
        SeedOrg(context);
        var user = new User
        {
            Id = Guid.NewGuid(), FirstName = "Anna", LastName = "Svensson",
            Email = "anna@test.com", PasswordHash = "hash", Role = Role.Employee, OrganizationId = OrgId
        };
        context.Users.Add(user);

        var shift = new Shift
        {
            Id = Guid.NewGuid(), UserId = user.Id, IsUpForSwap = true, OrganizationId = OrgId,
            StartTime = DateTime.UtcNow.AddDays(1).Date.AddHours(8),
            EndTime = DateTime.UtcNow.AddDays(1).Date.AddHours(16)
        };
        context.Shifts.Add(shift);

        context.SwapRequests.Add(new SwapRequest
        {
            Id = Guid.NewGuid(), ShiftId = shift.Id, RequestingUserId = user.Id,
            Status = SwapRequestStatus.Pending, CreatedAt = DateTime.UtcNow
        });
        context.SwapRequests.Add(new SwapRequest
        {
            Id = Guid.NewGuid(), ShiftId = shift.Id, RequestingUserId = user.Id,
            Status = SwapRequestStatus.Accepted, CreatedAt = DateTime.UtcNow
        });
        context.SwapRequests.Add(new SwapRequest
        {
            Id = Guid.NewGuid(), ShiftId = shift.Id, RequestingUserId = user.Id,
            Status = SwapRequestStatus.Declined, CreatedAt = DateTime.UtcNow
        });
        await context.SaveChangesAsync(CancellationToken.None);

        var handler = new GetAvailableSwapsHandler(context);
        var result = await handler.Handle(new GetAvailableSwapsQuery(OrgId), CancellationToken.None);

        result.Should().HaveCount(1);
        result[0].Status.Should().Be("Pending");

        TestDbContextFactory.Destroy(context);
    }

    [Fact]
    public async Task Handle_Should_Map_Shift_And_User_To_Dto()
    {
        var context = TestDbContextFactory.Create();
        SeedOrg(context);
        var user = new User
        {
            Id = Guid.NewGuid(), FirstName = "Anna", LastName = "Svensson",
            Email = "anna@test.com", PasswordHash = "hash", Role = Role.Employee, OrganizationId = OrgId
        };
        context.Users.Add(user);

        var shift = new Shift
        {
            Id = Guid.NewGuid(), UserId = user.Id, IsUpForSwap = true, OrganizationId = OrgId,
            StartTime = DateTime.UtcNow.AddDays(1).Date.AddHours(8),
            EndTime = DateTime.UtcNow.AddDays(1).Date.AddHours(16)
        };
        context.Shifts.Add(shift);

        context.SwapRequests.Add(new SwapRequest
        {
            Id = Guid.NewGuid(), ShiftId = shift.Id, RequestingUserId = user.Id,
            Status = SwapRequestStatus.Pending, CreatedAt = DateTime.UtcNow
        });
        await context.SaveChangesAsync(CancellationToken.None);

        var handler = new GetAvailableSwapsHandler(context);
        var result = await handler.Handle(new GetAvailableSwapsQuery(OrgId), CancellationToken.None);

        result.Should().HaveCount(1);
        result[0].Shift.Should().NotBeNull();
        result[0].Shift!.Id.Should().Be(shift.Id);
        result[0].RequestingUser.Should().NotBeNull();
        result[0].RequestingUser!.FirstName.Should().Be("Anna");

        TestDbContextFactory.Destroy(context);
    }

    [Fact]
    public async Task Handle_Should_Return_Empty_List_When_No_Pending_Requests()
    {
        var context = TestDbContextFactory.Create();
        var handler = new GetAvailableSwapsHandler(context);

        var result = await handler.Handle(new GetAvailableSwapsQuery(OrgId), CancellationToken.None);

        result.Should().BeEmpty();

        TestDbContextFactory.Destroy(context);
    }

    private static void SeedOrg(Infrastructure.AppDbContext context)
    {
        context.Organizations.Add(new Organization { Id = OrgId, Name = "Test Org" });
        context.SaveChanges();
    }
}
