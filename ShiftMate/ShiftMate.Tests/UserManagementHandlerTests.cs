using FluentAssertions;
using ShiftMate.Application.Users.Commands;
using ShiftMate.Domain;
using ShiftMate.Tests.Support;

namespace ShiftMate.Tests;

public class UserManagementHandlerTests
{
    // -------------------------------------------------------
    // DeleteUserHandler
    // -------------------------------------------------------

    [Fact]
    public async Task DeleteUser_Should_Remove_User()
    {
        // Arrange
        var context = TestDbContextFactory.Create();
        var managerId = Guid.NewGuid();
        var targetId = Guid.NewGuid();

        context.Users.Add(new User { Id = managerId, FirstName = "Manager", LastName = "Test", Email = "manager@test.com", PasswordHash = "hash", Role = Role.Manager });
        context.Users.Add(new User { Id = targetId, FirstName = "Anna", LastName = "Svensson", Email = "anna@test.com", PasswordHash = "hash", Role = Role.Employee });
        await context.SaveChangesAsync(CancellationToken.None);

        var handler = new DeleteUserHandler(context);

        // Act
        var result = await handler.Handle(new DeleteUserCommand { TargetUserId = targetId, RequestingUserId = managerId }, CancellationToken.None);

        // Assert
        result.Should().BeTrue();
        context.Users.Should().NotContain(u => u.Id == targetId);

        TestDbContextFactory.Destroy(context);
    }

    [Fact]
    public async Task DeleteUser_Should_Throw_When_Deleting_Self()
    {
        // Arrange
        var context = TestDbContextFactory.Create();
        var managerId = Guid.NewGuid();

        context.Users.Add(new User { Id = managerId, FirstName = "Manager", LastName = "Test", Email = "manager@test.com", PasswordHash = "hash", Role = Role.Manager });
        await context.SaveChangesAsync(CancellationToken.None);

        var handler = new DeleteUserHandler(context);

        // Act
        var act = async () => await handler.Handle(
            new DeleteUserCommand { TargetUserId = managerId, RequestingUserId = managerId },
            CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*radera*");

        TestDbContextFactory.Destroy(context);
    }

    [Fact]
    public async Task DeleteUser_Should_Throw_When_User_Not_Found()
    {
        // Arrange
        var context = TestDbContextFactory.Create();
        var managerId = Guid.NewGuid();
        var nonExistentId = Guid.NewGuid();

        context.Users.Add(new User { Id = managerId, FirstName = "Manager", LastName = "Test", Email = "manager@test.com", PasswordHash = "hash", Role = Role.Manager });
        await context.SaveChangesAsync(CancellationToken.None);

        var handler = new DeleteUserHandler(context);

        // Act
        var act = async () => await handler.Handle(
            new DeleteUserCommand { TargetUserId = nonExistentId, RequestingUserId = managerId },
            CancellationToken.None);

        // Assert
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
        // Arrange
        var context = TestDbContextFactory.Create();
        var managerId = Guid.NewGuid();
        var targetId = Guid.NewGuid();

        context.Users.Add(new User { Id = managerId, FirstName = "Manager", LastName = "Test", Email = "manager@test.com", PasswordHash = "hash", Role = Role.Manager });
        context.Users.Add(new User { Id = targetId, FirstName = "Anna", LastName = "Svensson", Email = "anna@test.com", PasswordHash = "hash", Role = Role.Employee });
        await context.SaveChangesAsync(CancellationToken.None);

        var handler = new UpdateUserRoleHandler(context);

        // Act
        var result = await handler.Handle(
            new UpdateUserRoleCommand { TargetUserId = targetId, NewRole = "Manager", RequestingUserId = managerId },
            CancellationToken.None);

        // Assert
        result.Should().BeTrue();
        var updatedUser = await context.Users.FindAsync(targetId);
        updatedUser!.Role.Should().Be(Role.Manager);

        TestDbContextFactory.Destroy(context);
    }

    [Fact]
    public async Task UpdateUserRole_Should_Throw_When_Updating_Self()
    {
        // Arrange
        var context = TestDbContextFactory.Create();
        var managerId = Guid.NewGuid();

        context.Users.Add(new User { Id = managerId, FirstName = "Manager", LastName = "Test", Email = "manager@test.com", PasswordHash = "hash", Role = Role.Manager });
        await context.SaveChangesAsync(CancellationToken.None);

        var handler = new UpdateUserRoleHandler(context);

        // Act
        var act = async () => await handler.Handle(
            new UpdateUserRoleCommand { TargetUserId = managerId, NewRole = "Employee", RequestingUserId = managerId },
            CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*ändra din egen roll*");

        TestDbContextFactory.Destroy(context);
    }

    [Fact]
    public async Task UpdateUserRole_Should_Throw_When_Invalid_Role()
    {
        // Arrange
        var context = TestDbContextFactory.Create();
        var managerId = Guid.NewGuid();
        var targetId = Guid.NewGuid();

        context.Users.Add(new User { Id = managerId, FirstName = "Manager", LastName = "Test", Email = "manager@test.com", PasswordHash = "hash", Role = Role.Manager });
        context.Users.Add(new User { Id = targetId, FirstName = "Anna", LastName = "Svensson", Email = "anna@test.com", PasswordHash = "hash", Role = Role.Employee });
        await context.SaveChangesAsync(CancellationToken.None);

        var handler = new UpdateUserRoleHandler(context);

        // Act
        var act = async () => await handler.Handle(
            new UpdateUserRoleCommand { TargetUserId = targetId, NewRole = "Superadmin", RequestingUserId = managerId },
            CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<ArgumentException>()
            .WithMessage("*Ogiltig roll*");

        TestDbContextFactory.Destroy(context);
    }
}
