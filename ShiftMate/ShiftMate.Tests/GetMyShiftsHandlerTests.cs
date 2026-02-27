using FluentAssertions;
using ShiftMate.Application.Shifts.Queries;
using ShiftMate.Domain;
using ShiftMate.Tests.Support;

namespace ShiftMate.Tests;

public class GetMyShiftsHandlerTests
{
    private static readonly Guid OrgId = Guid.NewGuid();

    [Fact]
    public async Task Handle_Should_Return_Only_Shifts_Belonging_To_User()
    {
        var context = TestDbContextFactory.Create();
        SeedOrg(context);
        var userId = Guid.NewGuid();
        var otherUserId = Guid.NewGuid();

        context.Users.Add(new User
        {
            Id = userId, FirstName = "Anna", LastName = "Svensson",
            Email = "anna@test.com", PasswordHash = "hash", Role = Role.Employee, OrganizationId = OrgId
        });
        context.Users.Add(new User
        {
            Id = otherUserId, FirstName = "Erik", LastName = "Eriksson",
            Email = "erik@test.com", PasswordHash = "hash", Role = Role.Employee, OrganizationId = OrgId
        });
        context.Shifts.Add(new Shift
        {
            Id = Guid.NewGuid(), UserId = userId, IsUpForSwap = false, OrganizationId = OrgId,
            StartTime = DateTime.UtcNow.AddDays(1).Date.AddHours(8),
            EndTime = DateTime.UtcNow.AddDays(1).Date.AddHours(16)
        });
        context.Shifts.Add(new Shift
        {
            Id = Guid.NewGuid(), UserId = otherUserId, IsUpForSwap = false, OrganizationId = OrgId,
            StartTime = DateTime.UtcNow.AddDays(1).Date.AddHours(8),
            EndTime = DateTime.UtcNow.AddDays(1).Date.AddHours(16)
        });
        context.Shifts.Add(new Shift
        {
            Id = Guid.NewGuid(), UserId = null, IsUpForSwap = false, OrganizationId = OrgId,
            StartTime = DateTime.UtcNow.AddDays(2).Date.AddHours(8),
            EndTime = DateTime.UtcNow.AddDays(2).Date.AddHours(16)
        });
        await context.SaveChangesAsync(CancellationToken.None);

        var handler = new GetMyShiftsHandler(context);
        var result = await handler.Handle(new GetMyShiftsQuery(userId, OrgId), CancellationToken.None);

        result.Should().HaveCount(1);
        result[0].UserId.Should().Be(userId);

        TestDbContextFactory.Destroy(context);
    }

    [Fact]
    public async Task Handle_Should_Return_Shifts_Sorted_By_StartTime()
    {
        var context = TestDbContextFactory.Create();
        SeedOrg(context);
        var userId = Guid.NewGuid();

        context.Users.Add(new User
        {
            Id = userId, FirstName = "Anna", LastName = "Svensson",
            Email = "anna@test.com", PasswordHash = "hash", Role = Role.Employee, OrganizationId = OrgId
        });
        context.Shifts.Add(new Shift
        {
            Id = Guid.NewGuid(), UserId = userId, IsUpForSwap = false, OrganizationId = OrgId,
            StartTime = DateTime.UtcNow.AddDays(3).Date.AddHours(8),
            EndTime = DateTime.UtcNow.AddDays(3).Date.AddHours(16)
        });
        context.Shifts.Add(new Shift
        {
            Id = Guid.NewGuid(), UserId = userId, IsUpForSwap = false, OrganizationId = OrgId,
            StartTime = DateTime.UtcNow.AddDays(1).Date.AddHours(8),
            EndTime = DateTime.UtcNow.AddDays(1).Date.AddHours(16)
        });
        await context.SaveChangesAsync(CancellationToken.None);

        var handler = new GetMyShiftsHandler(context);
        var result = await handler.Handle(new GetMyShiftsQuery(userId, OrgId), CancellationToken.None);

        result.Should().HaveCount(2);
        result[0].StartTime.Should().BeBefore(result[1].StartTime);

        TestDbContextFactory.Destroy(context);
    }

    [Fact]
    public async Task Handle_Should_Return_Empty_List_When_User_Has_No_Shifts()
    {
        var context = TestDbContextFactory.Create();
        var handler = new GetMyShiftsHandler(context);

        var result = await handler.Handle(new GetMyShiftsQuery(Guid.NewGuid(), OrgId), CancellationToken.None);

        result.Should().BeEmpty();

        TestDbContextFactory.Destroy(context);
    }

    private static void SeedOrg(Infrastructure.AppDbContext context)
    {
        context.Organizations.Add(new Organization { Id = OrgId, Name = "Test Org" });
        context.SaveChanges();
    }
}
