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

        result.Items.Should().HaveCount(2);
        result.TotalCount.Should().Be(2);
        result.Items[0].StartTime.Should().BeBefore(result.Items[1].StartTime);

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

        result.Items.Should().HaveCount(1);
        result.TotalCount.Should().Be(1);
        result.Items[0].UserId.Should().Be(user.Id);

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

        result.Items.Should().HaveCount(1);
        result.Items[0].User.Should().NotBeNull();
        result.Items[0].User!.FirstName.Should().Be("Anna");
        result.Items[0].User!.Email.Should().Be("anna@test.com");
        result.Items[0].IsUpForSwap.Should().BeTrue();

        TestDbContextFactory.Destroy(context);
    }

    [Fact]
    public async Task Handle_Should_Return_Empty_List_When_No_Shifts()
    {
        var context = TestDbContextFactory.Create();
        var handler = new GetAllShiftsHandler(context);

        var result = await handler.Handle(new GetAllShiftsQuery(OrgId), CancellationToken.None);

        result.Items.Should().BeEmpty();
        result.TotalCount.Should().Be(0);

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

        result.Items.Should().HaveCount(1);
        result.TotalCount.Should().Be(1);

        TestDbContextFactory.Destroy(context);
    }

    [Fact]
    public async Task Handle_Should_Paginate_When_Page_And_PageSize_Are_Set()
    {
        var context = TestDbContextFactory.Create();
        SeedOrg(context);

        // Skapa 5 pass
        for (int i = 0; i < 5; i++)
        {
            context.Shifts.Add(new Shift
            {
                Id = Guid.NewGuid(), UserId = null, IsUpForSwap = false, OrganizationId = OrgId,
                StartTime = DateTime.UtcNow.AddDays(i + 1).Date.AddHours(8),
                EndTime = DateTime.UtcNow.AddDays(i + 1).Date.AddHours(16)
            });
        }
        await context.SaveChangesAsync(CancellationToken.None);

        var handler = new GetAllShiftsHandler(context);

        // Hämta sida 1 med 2 per sida
        var page1 = await handler.Handle(new GetAllShiftsQuery(OrgId, Page: 1, PageSize: 2), CancellationToken.None);
        page1.Items.Should().HaveCount(2);
        page1.TotalCount.Should().Be(5);
        page1.Page.Should().Be(1);
        page1.PageSize.Should().Be(2);
        page1.TotalPages.Should().Be(3);

        // Hämta sida 3 (sista) med 2 per sida
        var page3 = await handler.Handle(new GetAllShiftsQuery(OrgId, Page: 3, PageSize: 2), CancellationToken.None);
        page3.Items.Should().HaveCount(1);
        page3.TotalCount.Should().Be(5);

        TestDbContextFactory.Destroy(context);
    }

    [Fact]
    public async Task Handle_Should_Return_All_When_No_Pagination_Params()
    {
        var context = TestDbContextFactory.Create();
        SeedOrg(context);

        for (int i = 0; i < 5; i++)
        {
            context.Shifts.Add(new Shift
            {
                Id = Guid.NewGuid(), UserId = null, IsUpForSwap = false, OrganizationId = OrgId,
                StartTime = DateTime.UtcNow.AddDays(i + 1).Date.AddHours(8),
                EndTime = DateTime.UtcNow.AddDays(i + 1).Date.AddHours(16)
            });
        }
        await context.SaveChangesAsync(CancellationToken.None);

        var handler = new GetAllShiftsHandler(context);
        var result = await handler.Handle(new GetAllShiftsQuery(OrgId), CancellationToken.None);

        result.Items.Should().HaveCount(5);
        result.TotalCount.Should().Be(5);

        TestDbContextFactory.Destroy(context);
    }

    private static void SeedOrg(Infrastructure.AppDbContext context)
    {
        context.Organizations.Add(new Organization { Id = OrgId, Name = "Test Org" });
        context.SaveChanges();
    }
}
