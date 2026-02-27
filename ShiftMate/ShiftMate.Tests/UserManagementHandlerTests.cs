using FluentAssertions;
using ShiftMate.Application.Users.Commands;
using ShiftMate.Domain;
using ShiftMate.Tests.Support;

namespace ShiftMate.Tests;

public class UserManagementHandlerTests
{
    private static readonly Guid OrgId = Guid.NewGuid();

    // -------------------------------------------------------
    // DeleteUserHandler
    // -------------------------------------------------------

    [Fact]
    public async Task DeleteUser_Should_Remove_User()
    {
        var context = TestDbContextFactory.Create();
        SeedOrg(context);
        var managerId = Guid.NewGuid();
        var targetId = Guid.NewGuid();

        context.Users.Add(new User { Id = managerId, FirstName = "Manager", LastName = "Test", Email = "manager@test.com", PasswordHash = "hash", Role = Role.Manager, OrganizationId = OrgId });
        context.Users.Add(new User { Id = targetId, FirstName = "Anna", LastName = "Svensson", Email = "anna@test.com", PasswordHash = "hash", Role = Role.Employee, OrganizationId = OrgId });
        await context.SaveChangesAsync(CancellationToken.None);

        var handler = new DeleteUserHandler(context);

        var result = await handler.Handle(new DeleteUserCommand { TargetUserId = targetId, RequestingUserId = managerId, OrganizationId = OrgId }, CancellationToken.None);

        result.Should().BeTrue();
        context.Users.Should().NotContain(u => u.Id == targetId);

        TestDbContextFactory.Destroy(context);
    }

    [Fact]
    public async Task DeleteUser_Should_Throw_When_Deleting_Self()
    {
        var context = TestDbContextFactory.Create();
        SeedOrg(context);
        var managerId = Guid.NewGuid();

        context.Users.Add(new User { Id = managerId, FirstName = "Manager", LastName = "Test", Email = "manager@test.com", PasswordHash = "hash", Role = Role.Manager, OrganizationId = OrgId });
        await context.SaveChangesAsync(CancellationToken.None);

        var handler = new DeleteUserHandler(context);

        var act = async () => await handler.Handle(
            new DeleteUserCommand { TargetUserId = managerId, RequestingUserId = managerId, OrganizationId = OrgId },
            CancellationToken.None);

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*radera*");

        TestDbContextFactory.Destroy(context);
    }

    [Fact]
    public async Task DeleteUser_Should_Throw_When_User_Not_Found()
    {
        var context = TestDbContextFactory.Create();
        SeedOrg(context);
        var managerId = Guid.NewGuid();
        var nonExistentId = Guid.NewGuid();

        context.Users.Add(new User { Id = managerId, FirstName = "Manager", LastName = "Test", Email = "manager@test.com", PasswordHash = "hash", Role = Role.Manager, OrganizationId = OrgId });
        await context.SaveChangesAsync(CancellationToken.None);

        var handler = new DeleteUserHandler(context);

        var act = async () => await handler.Handle(
            new DeleteUserCommand { TargetUserId = nonExistentId, RequestingUserId = managerId, OrganizationId = OrgId },
            CancellationToken.None);

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*hittades inte*");

        TestDbContextFactory.Destroy(context);
    }

    // -------------------------------------------------------
    // UpdateUserRoleHandler
    // -------------------------------------------------------

    [Fact]
    public async Task UpdateUserRole_Should_Change_Role()
    {
        var context = TestDbContextFactory.Create();
        SeedOrg(context);
        var managerId = Guid.NewGuid();
        var targetId = Guid.NewGuid();

        context.Users.Add(new User { Id = managerId, FirstName = "Manager", LastName = "Test", Email = "manager@test.com", PasswordHash = "hash", Role = Role.Manager, OrganizationId = OrgId });
        context.Users.Add(new User { Id = targetId, FirstName = "Anna", LastName = "Svensson", Email = "anna@test.com", PasswordHash = "hash", Role = Role.Employee, OrganizationId = OrgId });
        await context.SaveChangesAsync(CancellationToken.None);

        var handler = new UpdateUserRoleHandler(context);

        var result = await handler.Handle(
            new UpdateUserRoleCommand { TargetUserId = targetId, NewRole = "Manager", RequestingUserId = managerId, OrganizationId = OrgId },
            CancellationToken.None);

        result.Should().BeTrue();
        var updatedUser = await context.Users.FindAsync(targetId);
        updatedUser!.Role.Should().Be(Role.Manager);

        TestDbContextFactory.Destroy(context);
    }

    [Fact]
    public async Task UpdateUserRole_Should_Throw_When_Updating_Self()
    {
        var context = TestDbContextFactory.Create();
        SeedOrg(context);
        var managerId = Guid.NewGuid();

        context.Users.Add(new User { Id = managerId, FirstName = "Manager", LastName = "Test", Email = "manager@test.com", PasswordHash = "hash", Role = Role.Manager, OrganizationId = OrgId });
        await context.SaveChangesAsync(CancellationToken.None);

        var handler = new UpdateUserRoleHandler(context);

        var act = async () => await handler.Handle(
            new UpdateUserRoleCommand { TargetUserId = managerId, NewRole = "Employee", RequestingUserId = managerId, OrganizationId = OrgId },
            CancellationToken.None);

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*ändra din egen roll*");

        TestDbContextFactory.Destroy(context);
    }

    [Fact]
    public async Task UpdateUserRole_Should_Throw_When_Invalid_Role()
    {
        var context = TestDbContextFactory.Create();
        SeedOrg(context);
        var managerId = Guid.NewGuid();
        var targetId = Guid.NewGuid();

        context.Users.Add(new User { Id = managerId, FirstName = "Manager", LastName = "Test", Email = "manager@test.com", PasswordHash = "hash", Role = Role.Manager, OrganizationId = OrgId });
        context.Users.Add(new User { Id = targetId, FirstName = "Anna", LastName = "Svensson", Email = "anna@test.com", PasswordHash = "hash", Role = Role.Employee, OrganizationId = OrgId });
        await context.SaveChangesAsync(CancellationToken.None);

        var handler = new UpdateUserRoleHandler(context);

        var act = async () => await handler.Handle(
            new UpdateUserRoleCommand { TargetUserId = targetId, NewRole = "Superadmin", RequestingUserId = managerId, OrganizationId = OrgId },
            CancellationToken.None);

        await act.Should().ThrowAsync<ArgumentException>()
            .WithMessage("*Ogiltig roll*");

        TestDbContextFactory.Destroy(context);
    }

    private static void SeedOrg(Infrastructure.AppDbContext context)
    {
        context.Organizations.Add(new Organization { Id = OrgId, Name = "Test Org" });
        context.SaveChanges();
    }
}
