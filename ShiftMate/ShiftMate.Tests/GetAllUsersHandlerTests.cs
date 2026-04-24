using FluentAssertions;
using ShiftMate.Application.Users.Queries;
using ShiftMate.Domain.Entities;
using ShiftMate.Domain.Enums;
using ShiftMate.Tests.Support;

namespace ShiftMate.Tests;

public class GetAllUsersHandlerTests
{
    private static readonly Guid OrgId = Guid.NewGuid();

    [Fact]
    public async Task Handle_Should_Return_All_Users_As_Dtos()
    {
        var context = TestDbContextFactory.Create();
        SeedOrg(context);
        context.Users.Add(new User
        {
            Id = Guid.NewGuid(), FirstName = "Anna", LastName = "Svensson",
            Email = "anna@test.com", PasswordHash = "secrethash", Role = Role.Employee, OrganizationId = OrgId
        });
        context.Users.Add(new User
        {
            Id = Guid.NewGuid(), FirstName = "Erik", LastName = "Eriksson",
            Email = "erik@test.com", PasswordHash = "secrethash", Role = Role.Manager, OrganizationId = OrgId
        });
        await context.SaveChangesAsync(CancellationToken.None);

        var handler = new GetAllUsersHandler(context);
        var result = await handler.Handle(new GetAllUsersQuery(OrgId), CancellationToken.None);

        result.Items.Should().HaveCount(2);
        result.TotalCount.Should().Be(2);
        result.Items.Should().Contain(u => u.Email == "anna@test.com");
        result.Items.Should().Contain(u => u.Email == "erik@test.com");

        TestDbContextFactory.Destroy(context);
    }

    [Fact]
    public async Task Handle_Should_Not_Expose_PasswordHash()
    {
        var context = TestDbContextFactory.Create();
        SeedOrg(context);
        context.Users.Add(new User
        {
            Id = Guid.NewGuid(), FirstName = "Anna", LastName = "Svensson",
            Email = "anna@test.com", PasswordHash = "supersecret123", Role = Role.Employee, OrganizationId = OrgId
        });
        await context.SaveChangesAsync(CancellationToken.None);

        var handler = new GetAllUsersHandler(context);
        var result = await handler.Handle(new GetAllUsersQuery(OrgId), CancellationToken.None);

        result.Items.Should().HaveCount(1);
        var userDto = result.Items[0];
        userDto.FirstName.Should().Be("Anna");
        var properties = userDto.GetType().GetProperties();
        properties.Should().NotContain(p => p.Name == "PasswordHash");

        TestDbContextFactory.Destroy(context);
    }

    [Fact]
    public async Task Handle_Should_Return_Empty_List_When_No_Users()
    {
        var context = TestDbContextFactory.Create();
        var handler = new GetAllUsersHandler(context);

        var result = await handler.Handle(new GetAllUsersQuery(OrgId), CancellationToken.None);

        result.Items.Should().BeEmpty();
        result.TotalCount.Should().Be(0);

        TestDbContextFactory.Destroy(context);
    }

    [Fact]
    public async Task Handle_Should_Only_Return_Users_From_Same_Organization()
    {
        var context = TestDbContextFactory.Create();
        SeedOrg(context);
        var otherOrgId = Guid.NewGuid();
        context.Organizations.Add(new Organization { Id = otherOrgId, Name = "Other Org" });

        context.Users.Add(new User
        {
            Id = Guid.NewGuid(), FirstName = "Anna", LastName = "Svensson",
            Email = "anna@test.com", PasswordHash = "hash", Role = Role.Employee, OrganizationId = OrgId
        });
        context.Users.Add(new User
        {
            Id = Guid.NewGuid(), FirstName = "Erik", LastName = "Eriksson",
            Email = "erik@test.com", PasswordHash = "hash", Role = Role.Employee, OrganizationId = otherOrgId
        });
        await context.SaveChangesAsync(CancellationToken.None);

        var handler = new GetAllUsersHandler(context);
        var result = await handler.Handle(new GetAllUsersQuery(OrgId), CancellationToken.None);

        result.Items.Should().HaveCount(1);
        result.TotalCount.Should().Be(1);
        result.Items[0].Email.Should().Be("anna@test.com");

        TestDbContextFactory.Destroy(context);
    }

    [Fact]
    public async Task Handle_Should_Paginate_When_Page_And_PageSize_Are_Set()
    {
        var context = TestDbContextFactory.Create();
        SeedOrg(context);

        for (int i = 0; i < 5; i++)
        {
            context.Users.Add(new User
            {
                Id = Guid.NewGuid(), FirstName = $"User{i}", LastName = "Test",
                Email = $"user{i}@test.com", PasswordHash = "hash", Role = Role.Employee, OrganizationId = OrgId
            });
        }
        await context.SaveChangesAsync(CancellationToken.None);

        var handler = new GetAllUsersHandler(context);

        var page1 = await handler.Handle(new GetAllUsersQuery(OrgId, Page: 1, PageSize: 2), CancellationToken.None);
        page1.Items.Should().HaveCount(2);
        page1.TotalCount.Should().Be(5);
        page1.Page.Should().Be(1);
        page1.PageSize.Should().Be(2);
        page1.TotalPages.Should().Be(3);

        var page3 = await handler.Handle(new GetAllUsersQuery(OrgId, Page: 3, PageSize: 2), CancellationToken.None);
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
            context.Users.Add(new User
            {
                Id = Guid.NewGuid(), FirstName = $"User{i}", LastName = "Test",
                Email = $"user{i}@test.com", PasswordHash = "hash", Role = Role.Employee, OrganizationId = OrgId
            });
        }
        await context.SaveChangesAsync(CancellationToken.None);

        var handler = new GetAllUsersHandler(context);
        var result = await handler.Handle(new GetAllUsersQuery(OrgId), CancellationToken.None);

        result.Items.Should().HaveCount(5);
        result.TotalCount.Should().Be(5);

        TestDbContextFactory.Destroy(context);
    }

    [Fact]
    public async Task Handle_Should_Exclude_Inactive_Users()
    {
        var context = TestDbContextFactory.Create();
        SeedOrg(context);

        context.Users.Add(new User
        {
            Id = Guid.NewGuid(), FirstName = "Anna", LastName = "Svensson",
            Email = "anna@test.com", PasswordHash = "hash", Role = Role.Employee, OrganizationId = OrgId, IsActive = true
        });
        context.Users.Add(new User
        {
            Id = Guid.NewGuid(), FirstName = "Erik", LastName = "Eriksson",
            Email = "erik@test.com", PasswordHash = "hash", Role = Role.Employee, OrganizationId = OrgId, IsActive = false, DeactivatedAt = DateTime.UtcNow
        });
        await context.SaveChangesAsync(CancellationToken.None);

        var handler = new GetAllUsersHandler(context);
        var result = await handler.Handle(new GetAllUsersQuery(OrgId), CancellationToken.None);

        result.Items.Should().HaveCount(1);
        result.Items[0].Email.Should().Be("anna@test.com");

        TestDbContextFactory.Destroy(context);
    }

    private static void SeedOrg(Infrastructure.AppDbContext context)
    {
        context.Organizations.Add(new Organization { Id = OrgId, Name = "Test Org" });
        context.SaveChanges();
    }
}
