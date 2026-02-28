using FluentAssertions;
using ShiftMate.Application.Organizations.Commands;
using ShiftMate.Application.Organizations.Queries;
using ShiftMate.Domain;
using ShiftMate.Tests.Support;

namespace ShiftMate.Tests;

public class OrganizationHandlerTests
{
    // ================================================
    // GetAllOrganizationsDetailHandler
    // ================================================

    [Fact]
    public async Task GetAllDetail_Should_Return_All_Orgs_With_UserCount()
    {
        var context = TestDbContextFactory.Create();

        var org1 = new Organization { Id = Guid.NewGuid(), Name = "Org A", CreatedAt = DateTime.UtcNow };
        var org2 = new Organization { Id = Guid.NewGuid(), Name = "Org B", CreatedAt = DateTime.UtcNow };
        context.Organizations.AddRange(org1, org2);

        context.Users.Add(new User { Id = Guid.NewGuid(), FirstName = "A", LastName = "B", Email = "a@test.com", PasswordHash = "h", Role = Role.Employee, OrganizationId = org1.Id });
        context.Users.Add(new User { Id = Guid.NewGuid(), FirstName = "C", LastName = "D", Email = "c@test.com", PasswordHash = "h", Role = Role.Employee, OrganizationId = org1.Id });
        await context.SaveChangesAsync(CancellationToken.None);

        var handler = new GetAllOrganizationsDetailHandler(context);
        var result = await handler.Handle(new GetAllOrganizationsDetailQuery(), CancellationToken.None);

        result.Should().HaveCount(2);
        result.First(o => o.Name == "Org A").UserCount.Should().Be(2);
        result.First(o => o.Name == "Org B").UserCount.Should().Be(0);

        TestDbContextFactory.Destroy(context);
    }

    [Fact]
    public async Task GetAllDetail_Should_Return_Empty_When_No_Orgs()
    {
        var context = TestDbContextFactory.Create();

        var handler = new GetAllOrganizationsDetailHandler(context);
        var result = await handler.Handle(new GetAllOrganizationsDetailQuery(), CancellationToken.None);

        result.Should().BeEmpty();

        TestDbContextFactory.Destroy(context);
    }

    // ================================================
    // CreateOrganizationHandler
    // ================================================

    [Fact]
    public async Task Create_Should_Add_Organization()
    {
        var context = TestDbContextFactory.Create();

        var handler = new CreateOrganizationHandler(context);
        var id = await handler.Handle(new CreateOrganizationCommand("Nytt Företag"), CancellationToken.None);

        id.Should().NotBeEmpty();
        context.Organizations.Should().ContainSingle(o => o.Name == "Nytt Företag");

        TestDbContextFactory.Destroy(context);
    }

    [Fact]
    public async Task Create_Should_Trim_Name()
    {
        var context = TestDbContextFactory.Create();

        var handler = new CreateOrganizationHandler(context);
        await handler.Handle(new CreateOrganizationCommand("  Trimmat Namn  "), CancellationToken.None);

        context.Organizations.Should().ContainSingle(o => o.Name == "Trimmat Namn");

        TestDbContextFactory.Destroy(context);
    }

    [Fact]
    public async Task Create_Should_Throw_When_Duplicate_Name()
    {
        var context = TestDbContextFactory.Create();
        context.Organizations.Add(new Organization { Id = Guid.NewGuid(), Name = "Befintlig Org", CreatedAt = DateTime.UtcNow });
        await context.SaveChangesAsync(CancellationToken.None);

        var handler = new CreateOrganizationHandler(context);
        var act = () => handler.Handle(new CreateOrganizationCommand("befintlig org"), CancellationToken.None);

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*redan*");

        TestDbContextFactory.Destroy(context);
    }

    // ================================================
    // UpdateOrganizationHandler
    // ================================================

    [Fact]
    public async Task Update_Should_Change_Name()
    {
        var context = TestDbContextFactory.Create();
        var orgId = Guid.NewGuid();
        context.Organizations.Add(new Organization { Id = orgId, Name = "Gammalt Namn", CreatedAt = DateTime.UtcNow });
        await context.SaveChangesAsync(CancellationToken.None);

        var handler = new UpdateOrganizationHandler(context);
        await handler.Handle(new UpdateOrganizationCommand(orgId, "Nytt Namn"), CancellationToken.None);

        context.Organizations.First().Name.Should().Be("Nytt Namn");

        TestDbContextFactory.Destroy(context);
    }

    [Fact]
    public async Task Update_Should_Throw_When_Not_Found()
    {
        var context = TestDbContextFactory.Create();

        var handler = new UpdateOrganizationHandler(context);
        var act = () => handler.Handle(new UpdateOrganizationCommand(Guid.NewGuid(), "Test"), CancellationToken.None);

        await act.Should().ThrowAsync<Exception>()
            .WithMessage("*hittades inte*");

        TestDbContextFactory.Destroy(context);
    }

    [Fact]
    public async Task Update_Should_Throw_When_Duplicate_Name()
    {
        var context = TestDbContextFactory.Create();
        var org1Id = Guid.NewGuid();
        context.Organizations.Add(new Organization { Id = org1Id, Name = "Org A", CreatedAt = DateTime.UtcNow });
        context.Organizations.Add(new Organization { Id = Guid.NewGuid(), Name = "Org B", CreatedAt = DateTime.UtcNow });
        await context.SaveChangesAsync(CancellationToken.None);

        var handler = new UpdateOrganizationHandler(context);
        var act = () => handler.Handle(new UpdateOrganizationCommand(org1Id, "org b"), CancellationToken.None);

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*redan*");

        TestDbContextFactory.Destroy(context);
    }

    // ================================================
    // DeleteOrganizationHandler
    // ================================================

    [Fact]
    public async Task Delete_Should_Remove_Organization()
    {
        var context = TestDbContextFactory.Create();
        var orgId = Guid.NewGuid();
        context.Organizations.Add(new Organization { Id = orgId, Name = "Ta Bort Mig", CreatedAt = DateTime.UtcNow });
        await context.SaveChangesAsync(CancellationToken.None);

        var handler = new DeleteOrganizationHandler(context);
        await handler.Handle(new DeleteOrganizationCommand(orgId), CancellationToken.None);

        context.Organizations.Should().BeEmpty();

        TestDbContextFactory.Destroy(context);
    }

    [Fact]
    public async Task Delete_Should_Throw_When_Not_Found()
    {
        var context = TestDbContextFactory.Create();

        var handler = new DeleteOrganizationHandler(context);
        var act = () => handler.Handle(new DeleteOrganizationCommand(Guid.NewGuid()), CancellationToken.None);

        await act.Should().ThrowAsync<Exception>()
            .WithMessage("*hittades inte*");

        TestDbContextFactory.Destroy(context);
    }

    [Fact]
    public async Task Delete_Should_Cascade_Delete_Users_And_Shifts()
    {
        var context = TestDbContextFactory.Create();
        var orgId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var shiftId = Guid.NewGuid();

        context.Organizations.Add(new Organization { Id = orgId, Name = "Radera Allt", CreatedAt = DateTime.UtcNow });
        context.Users.Add(new User { Id = userId, FirstName = "Test", LastName = "User", Email = "test@test.com", PasswordHash = "h", Role = Role.Employee, OrganizationId = orgId });
        context.Shifts.Add(new Shift { Id = shiftId, StartTime = DateTime.UtcNow, EndTime = DateTime.UtcNow.AddHours(8), OrganizationId = orgId, UserId = userId });
        await context.SaveChangesAsync(CancellationToken.None);

        var handler = new DeleteOrganizationHandler(context);
        await handler.Handle(new DeleteOrganizationCommand(orgId), CancellationToken.None);

        context.Organizations.Should().BeEmpty();
        context.Users.Should().BeEmpty();
        context.Shifts.Should().BeEmpty();

        TestDbContextFactory.Destroy(context);
    }
}
