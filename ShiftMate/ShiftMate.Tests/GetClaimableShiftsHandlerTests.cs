using FluentAssertions;
using ShiftMate.Application.Shifts.Queries;
using ShiftMate.Domain;
using ShiftMate.Tests.Support;

namespace ShiftMate.Tests;

public class GetClaimableShiftsHandlerTests
{
    private static readonly Guid OrgId = Guid.NewGuid();

    [Fact]
    public async Task Handle_Should_Return_Shifts_With_No_Owner()
    {
        var context = TestDbContextFactory.Create();
        SeedOrg(context);
        var user = new User
        {
            Id = Guid.NewGuid(), FirstName = "Anna", LastName = "Svensson",
            Email = "anna@test.com", PasswordHash = "hash", Role = Role.Employee, OrganizationId = OrgId
        };
        context.Users.Add(user);
        context.Shifts.Add(new Shift
        {
            Id = Guid.NewGuid(), UserId = null, IsUpForSwap = false, OrganizationId = OrgId,
            StartTime = DateTime.UtcNow.AddDays(1).Date.AddHours(8),
            EndTime = DateTime.UtcNow.AddDays(1).Date.AddHours(16)
        });
        context.Shifts.Add(new Shift
        {
            Id = Guid.NewGuid(), UserId = user.Id, IsUpForSwap = false, OrganizationId = OrgId,
            StartTime = DateTime.UtcNow.AddDays(2).Date.AddHours(8),
            EndTime = DateTime.UtcNow.AddDays(2).Date.AddHours(16)
        });
        await context.SaveChangesAsync(CancellationToken.None);

        var handler = new GetClaimableShiftsHandler(context);
        var result = await handler.Handle(new GetClaimableShiftsQuery(OrgId), CancellationToken.None);

        result.Should().HaveCount(1);
        result[0].UserId.Should().BeNull();

        TestDbContextFactory.Destroy(context);
    }

    [Fact]
    public async Task Handle_Should_Return_Shifts_Marked_As_Up_For_Swap()
    {
        var context = TestDbContextFactory.Create();
        SeedOrg(context);
        var user = new User
        {
            Id = Guid.NewGuid(), FirstName = "Anna", LastName = "Svensson",
            Email = "anna@test.com", PasswordHash = "hash", Role = Role.Employee, OrganizationId = OrgId
        };
        context.Users.Add(user);
        context.Shifts.Add(new Shift
        {
            Id = Guid.NewGuid(), UserId = user.Id, IsUpForSwap = true, OrganizationId = OrgId,
            StartTime = DateTime.UtcNow.AddDays(1).Date.AddHours(8),
            EndTime = DateTime.UtcNow.AddDays(1).Date.AddHours(16)
        });
        await context.SaveChangesAsync(CancellationToken.None);

        var handler = new GetClaimableShiftsHandler(context);
        var result = await handler.Handle(new GetClaimableShiftsQuery(OrgId), CancellationToken.None);

        result.Should().HaveCount(1);
        result[0].IsUpForSwap.Should().BeTrue();
        result[0].User.Should().NotBeNull();
        result[0].User!.FirstName.Should().Be("Anna");

        TestDbContextFactory.Destroy(context);
    }

    [Fact]
    public async Task Handle_Should_Not_Return_Assigned_Shifts_Not_Up_For_Swap()
    {
        var context = TestDbContextFactory.Create();
        SeedOrg(context);
        var user = new User
        {
            Id = Guid.NewGuid(), FirstName = "Anna", LastName = "Svensson",
            Email = "anna@test.com", PasswordHash = "hash", Role = Role.Employee, OrganizationId = OrgId
        };
        context.Users.Add(user);
        context.Shifts.Add(new Shift
        {
            Id = Guid.NewGuid(), UserId = user.Id, IsUpForSwap = false, OrganizationId = OrgId,
            StartTime = DateTime.UtcNow.AddDays(1).Date.AddHours(8),
            EndTime = DateTime.UtcNow.AddDays(1).Date.AddHours(16)
        });
        await context.SaveChangesAsync(CancellationToken.None);

        var handler = new GetClaimableShiftsHandler(context);
        var result = await handler.Handle(new GetClaimableShiftsQuery(OrgId), CancellationToken.None);

        result.Should().BeEmpty();

        TestDbContextFactory.Destroy(context);
    }

    [Fact]
    public async Task Handle_Should_Return_Shifts_Sorted_By_StartTime()
    {
        var context = TestDbContextFactory.Create();
        SeedOrg(context);
        context.Shifts.Add(new Shift
        {
            Id = Guid.NewGuid(), UserId = null, IsUpForSwap = false, OrganizationId = OrgId,
            StartTime = DateTime.UtcNow.AddDays(3).Date.AddHours(8),
            EndTime = DateTime.UtcNow.AddDays(3).Date.AddHours(16)
        });
        context.Shifts.Add(new Shift
        {
            Id = Guid.NewGuid(), UserId = null, IsUpForSwap = false, OrganizationId = OrgId,
            StartTime = DateTime.UtcNow.AddDays(1).Date.AddHours(8),
            EndTime = DateTime.UtcNow.AddDays(1).Date.AddHours(16)
        });
        await context.SaveChangesAsync(CancellationToken.None);

        var handler = new GetClaimableShiftsHandler(context);
        var result = await handler.Handle(new GetClaimableShiftsQuery(OrgId), CancellationToken.None);

        result.Should().HaveCount(2);
        result[0].StartTime.Should().BeBefore(result[1].StartTime);

        TestDbContextFactory.Destroy(context);
    }

    private static void SeedOrg(Infrastructure.AppDbContext context)
    {
        context.Organizations.Add(new Organization { Id = OrgId, Name = "Test Org" });
        context.SaveChanges();
    }
}
