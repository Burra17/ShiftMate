using FluentAssertions;
using ShiftMate.Application.Users.Queries;
using ShiftMate.Domain;
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

        result.Should().HaveCount(2);
        result.Should().Contain(u => u.Email == "anna@test.com");
        result.Should().Contain(u => u.Email == "erik@test.com");

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

        result.Should().HaveCount(1);
        var userDto = result[0];
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

        result.Should().BeEmpty();

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

        result.Should().HaveCount(1);
        result[0].Email.Should().Be("anna@test.com");

        TestDbContextFactory.Destroy(context);
    }

    private static void SeedOrg(Infrastructure.AppDbContext context)
    {
        context.Organizations.Add(new Organization { Id = OrgId, Name = "Test Org" });
        context.SaveChanges();
    }
}
