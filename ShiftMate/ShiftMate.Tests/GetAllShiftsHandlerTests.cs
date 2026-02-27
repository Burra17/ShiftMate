using FluentAssertions;
using ShiftMate.Application.Shifts.Queries;
using ShiftMate.Domain;
using ShiftMate.Tests.Support;

namespace ShiftMate.Tests;

public class GetAllShiftsHandlerTests
{
    private static readonly Guid OrgId = Guid.NewGuid();

    [Fact]
    public async Task Handle_Should_Return_All_Shifts_Sorted_By_StartTime()
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
            StartTime = DateTime.UtcNow.AddDays(2).Date.AddHours(8),
            EndTime = DateTime.UtcNow.AddDays(2).Date.AddHours(16)
        });
        context.Shifts.Add(new Shift
        {
            Id = Guid.NewGuid(), UserId = null, IsUpForSwap = false, OrganizationId = OrgId,
            StartTime = DateTime.UtcNow.AddDays(1).Date.AddHours(8),
            EndTime = DateTime.UtcNow.AddDays(1).Date.AddHours(16)
        });
        await context.SaveChangesAsync(CancellationToken.None);

        var handler = new GetAllShiftsHandler(context);
        var result = await handler.Handle(new GetAllShiftsQuery(OrgId), CancellationToken.None);

        result.Should().HaveCount(2);
        result[0].StartTime.Should().BeBefore(result[1].StartTime);

        TestDbContextFactory.Destroy(context);
    }

    [Fact]
    public async Task Handle_Should_Filter_Only_With_Users_When_Flag_Is_True()
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
        context.Shifts.Add(new Shift
        {
            Id = Guid.NewGuid(), UserId = null, IsUpForSwap = false, OrganizationId = OrgId,
            StartTime = DateTime.UtcNow.AddDays(2).Date.AddHours(8),
            EndTime = DateTime.UtcNow.AddDays(2).Date.AddHours(16)
        });
        await context.SaveChangesAsync(CancellationToken.None);

        var handler = new GetAllShiftsHandler(context);
        var result = await handler.Handle(new GetAllShiftsQuery(OrgId, OnlyWithUsers: true), CancellationToken.None);

        result.Should().HaveCount(1);
        result[0].UserId.Should().Be(user.Id);

        TestDbContextFactory.Destroy(context);
    }

    [Fact]
    public async Task Handle_Should_Map_User_To_Dto_When_User_Exists()
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

        var handler = new GetAllShiftsHandler(context);
        var result = await handler.Handle(new GetAllShiftsQuery(OrgId), CancellationToken.None);

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
        var context = TestDbContextFactory.Create();
        var handler = new GetAllShiftsHandler(context);

        var result = await handler.Handle(new GetAllShiftsQuery(OrgId), CancellationToken.None);

        result.Should().BeEmpty();

        TestDbContextFactory.Destroy(context);
    }

    [Fact]
    public async Task Handle_Should_Only_Return_Shifts_From_Same_Organization()
    {
        var context = TestDbContextFactory.Create();
        SeedOrg(context);
        var otherOrgId = Guid.NewGuid();
        context.Organizations.Add(new Organization { Id = otherOrgId, Name = "Other Org" });

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
        context.Shifts.Add(new Shift
        {
            Id = Guid.NewGuid(), UserId = null, IsUpForSwap = false, OrganizationId = otherOrgId,
            StartTime = DateTime.UtcNow.AddDays(1).Date.AddHours(8),
            EndTime = DateTime.UtcNow.AddDays(1).Date.AddHours(16)
        });
        await context.SaveChangesAsync(CancellationToken.None);

        var handler = new GetAllShiftsHandler(context);
        var result = await handler.Handle(new GetAllShiftsQuery(OrgId), CancellationToken.None);

        result.Should().HaveCount(1);

        TestDbContextFactory.Destroy(context);
    }

    private static void SeedOrg(Infrastructure.AppDbContext context)
    {
        context.Organizations.Add(new Organization { Id = OrgId, Name = "Test Org" });
        context.SaveChanges();
    }
}
