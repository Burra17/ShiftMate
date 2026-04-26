using FluentAssertions;
using FluentValidation;
using FluentValidation.Results;
using Moq;
using ShiftMate.Application.Users.Commands.DeleteUser;
using ShiftMate.Application.Users.Commands.UpdateUserRole;
using ShiftMate.Domain.Entities;
using ShiftMate.Domain.Enums;
using ShiftMate.Tests.Support;

namespace ShiftMate.Tests;

public class UserManagementHandlerTests
{
    private static readonly Guid OrgId = Guid.NewGuid();

    // -------------------------------------------------------
    // DeleteUserHandler (Soft Delete)
    // -------------------------------------------------------

    [Fact]
    public async Task DeleteUser_Should_Deactivate_User()
    {
        var context = TestDbContextFactory.Create();
        SeedOrg(context);
        var managerId = Guid.NewGuid();
        var targetId = Guid.NewGuid();

        context.Users.Add(new User { Id = managerId, FirstName = "Manager", LastName = "Test", Email = "manager@test.com", PasswordHash = "hash", Role = Role.Manager, OrganizationId = OrgId });
        context.Users.Add(new User { Id = targetId, FirstName = "Anna", LastName = "Svensson", Email = "anna@test.com", PasswordHash = "hash", Role = Role.Employee, OrganizationId = OrgId });
        await context.SaveChangesAsync(CancellationToken.None);

        var handler = new DeleteUserCommandHandler(context);

        var result = await handler.Handle(new DeleteUserCommand { TargetUserId = targetId, RequestingUserId = managerId, OrganizationId = OrgId }, CancellationToken.None);

        result.Should().BeTrue();
        var user = await context.Users.FindAsync(targetId);
        user.Should().NotBeNull();
        user!.IsActive.Should().BeFalse();
        user.DeactivatedAt.Should().NotBeNull();

        TestDbContextFactory.Destroy(context);
    }

    [Fact]
    public async Task DeleteUser_Should_Unassign_Shifts()
    {
        var context = TestDbContextFactory.Create();
        SeedOrg(context);
        var managerId = Guid.NewGuid();
        var targetId = Guid.NewGuid();
        var shiftId = Guid.NewGuid();

        context.Users.Add(new User { Id = managerId, FirstName = "Manager", LastName = "Test", Email = "manager@test.com", PasswordHash = "hash", Role = Role.Manager, OrganizationId = OrgId });
        context.Users.Add(new User { Id = targetId, FirstName = "Anna", LastName = "Svensson", Email = "anna@test.com", PasswordHash = "hash", Role = Role.Employee, OrganizationId = OrgId });
        context.Shifts.Add(new Shift { Id = shiftId, UserId = targetId, StartTime = DateTime.UtcNow, EndTime = DateTime.UtcNow.AddHours(8), IsUpForSwap = true, OrganizationId = OrgId });
        await context.SaveChangesAsync(CancellationToken.None);

        var handler = new DeleteUserCommandHandler(context);
        await handler.Handle(new DeleteUserCommand { TargetUserId = targetId, RequestingUserId = managerId, OrganizationId = OrgId }, CancellationToken.None);

        var shift = await context.Shifts.FindAsync(shiftId);
        shift!.UserId.Should().BeNull();
        shift.IsUpForSwap.Should().BeFalse();

        TestDbContextFactory.Destroy(context);
    }

    [Fact]
    public async Task DeleteUser_Should_Cancel_Pending_SwapRequests()
    {
        var context = TestDbContextFactory.Create();
        SeedOrg(context);
        var managerId = Guid.NewGuid();
        var targetId = Guid.NewGuid();
        var otherId = Guid.NewGuid();
        var shiftId1 = Guid.NewGuid();
        var shiftId2 = Guid.NewGuid();
        var swapId1 = Guid.NewGuid();
        var swapId2 = Guid.NewGuid();

        context.Users.Add(new User { Id = managerId, FirstName = "Manager", LastName = "Test", Email = "manager@test.com", PasswordHash = "hash", Role = Role.Manager, OrganizationId = OrgId });
        context.Users.Add(new User { Id = targetId, FirstName = "Anna", LastName = "Svensson", Email = "anna@test.com", PasswordHash = "hash", Role = Role.Employee, OrganizationId = OrgId });
        context.Users.Add(new User { Id = otherId, FirstName = "Erik", LastName = "Johansson", Email = "erik@test.com", PasswordHash = "hash", Role = Role.Employee, OrganizationId = OrgId });
        context.Shifts.Add(new Shift { Id = shiftId1, UserId = targetId, StartTime = DateTime.UtcNow, EndTime = DateTime.UtcNow.AddHours(8), OrganizationId = OrgId });
        context.Shifts.Add(new Shift { Id = shiftId2, UserId = otherId, StartTime = DateTime.UtcNow, EndTime = DateTime.UtcNow.AddHours(8), OrganizationId = OrgId });
        // Swap where target user is requesting
        context.SwapRequests.Add(new SwapRequest { Id = swapId1, ShiftId = shiftId1, RequestingUserId = targetId, TargetUserId = otherId, Status = SwapRequestStatus.Pending });
        // Swap where target user is the target
        context.SwapRequests.Add(new SwapRequest { Id = swapId2, ShiftId = shiftId2, RequestingUserId = otherId, TargetUserId = targetId, Status = SwapRequestStatus.Pending });
        await context.SaveChangesAsync(CancellationToken.None);

        var handler = new DeleteUserCommandHandler(context);
        await handler.Handle(new DeleteUserCommand { TargetUserId = targetId, RequestingUserId = managerId, OrganizationId = OrgId }, CancellationToken.None);

        var swap1 = await context.SwapRequests.FindAsync(swapId1);
        var swap2 = await context.SwapRequests.FindAsync(swapId2);
        swap1!.Status.Should().Be(SwapRequestStatus.Cancelled);
        swap2!.Status.Should().Be(SwapRequestStatus.Cancelled);

        TestDbContextFactory.Destroy(context);
    }

    [Fact]
    public async Task DeleteUser_Should_Throw_When_Deactivating_Self()
    {
        var context = TestDbContextFactory.Create();
        SeedOrg(context);
        var managerId = Guid.NewGuid();

        context.Users.Add(new User { Id = managerId, FirstName = "Manager", LastName = "Test", Email = "manager@test.com", PasswordHash = "hash", Role = Role.Manager, OrganizationId = OrgId });
        await context.SaveChangesAsync(CancellationToken.None);

        var handler = new DeleteUserCommandHandler(context);

        var act = async () => await handler.Handle(
            new DeleteUserCommand { TargetUserId = managerId, RequestingUserId = managerId, OrganizationId = OrgId },
            CancellationToken.None);

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*inaktivera*");

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

        var handler = new DeleteUserCommandHandler(context);

        var act = async () => await handler.Handle(
            new DeleteUserCommand { TargetUserId = nonExistentId, RequestingUserId = managerId, OrganizationId = OrgId },
            CancellationToken.None);

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*hittades inte*");

        TestDbContextFactory.Destroy(context);
    }

    [Fact]
    public async Task DeleteUser_Should_Throw_When_Wrong_Organization()
    {
        var context = TestDbContextFactory.Create();
        SeedOrg(context);
        var managerId = Guid.NewGuid();
        var targetId = Guid.NewGuid();
        var otherOrgId = Guid.NewGuid();

        context.Organizations.Add(new Organization { Id = otherOrgId, Name = "Other Org" });
        context.Users.Add(new User { Id = managerId, FirstName = "Manager", LastName = "Test", Email = "manager@test.com", PasswordHash = "hash", Role = Role.Manager, OrganizationId = OrgId });
        context.Users.Add(new User { Id = targetId, FirstName = "Anna", LastName = "Svensson", Email = "anna@test.com", PasswordHash = "hash", Role = Role.Employee, OrganizationId = otherOrgId });
        await context.SaveChangesAsync(CancellationToken.None);

        var handler = new DeleteUserCommandHandler(context);

        var act = async () => await handler.Handle(
            new DeleteUserCommand { TargetUserId = targetId, RequestingUserId = managerId, OrganizationId = OrgId },
            CancellationToken.None);

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*tillhör inte*");

        TestDbContextFactory.Destroy(context);
    }

    [Fact]
    public async Task DeleteUser_Should_Throw_When_Already_Deactivated()
    {
        var context = TestDbContextFactory.Create();
        SeedOrg(context);
        var managerId = Guid.NewGuid();
        var targetId = Guid.NewGuid();

        context.Users.Add(new User { Id = managerId, FirstName = "Manager", LastName = "Test", Email = "manager@test.com", PasswordHash = "hash", Role = Role.Manager, OrganizationId = OrgId });
        context.Users.Add(new User { Id = targetId, FirstName = "Anna", LastName = "Svensson", Email = "anna@test.com", PasswordHash = "hash", Role = Role.Employee, OrganizationId = OrgId, IsActive = false, DeactivatedAt = DateTime.UtcNow });
        await context.SaveChangesAsync(CancellationToken.None);

        var handler = new DeleteUserCommandHandler(context);

        var act = async () => await handler.Handle(
            new DeleteUserCommand { TargetUserId = targetId, RequestingUserId = managerId, OrganizationId = OrgId },
            CancellationToken.None);

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*redan inaktiverad*");

        TestDbContextFactory.Destroy(context);
    }

    [Fact]
    public async Task DeleteUser_Should_Not_Cancel_Non_Pending_SwapRequests()
    {
        var context = TestDbContextFactory.Create();
        SeedOrg(context);
        var managerId = Guid.NewGuid();
        var targetId = Guid.NewGuid();
        var otherId = Guid.NewGuid();
        var shiftId = Guid.NewGuid();
        var acceptedSwapId = Guid.NewGuid();

        context.Users.Add(new User { Id = managerId, FirstName = "Manager", LastName = "Test", Email = "manager@test.com", PasswordHash = "hash", Role = Role.Manager, OrganizationId = OrgId });
        context.Users.Add(new User { Id = targetId, FirstName = "Anna", LastName = "Svensson", Email = "anna@test.com", PasswordHash = "hash", Role = Role.Employee, OrganizationId = OrgId });
        context.Users.Add(new User { Id = otherId, FirstName = "Erik", LastName = "Johansson", Email = "erik@test.com", PasswordHash = "hash", Role = Role.Employee, OrganizationId = OrgId });
        context.Shifts.Add(new Shift { Id = shiftId, UserId = targetId, StartTime = DateTime.UtcNow, EndTime = DateTime.UtcNow.AddHours(8), OrganizationId = OrgId });
        context.SwapRequests.Add(new SwapRequest { Id = acceptedSwapId, ShiftId = shiftId, RequestingUserId = targetId, TargetUserId = otherId, Status = SwapRequestStatus.Accepted });
        await context.SaveChangesAsync(CancellationToken.None);

        var handler = new DeleteUserCommandHandler(context);
        await handler.Handle(new DeleteUserCommand { TargetUserId = targetId, RequestingUserId = managerId, OrganizationId = OrgId }, CancellationToken.None);

        var swap = await context.SwapRequests.FindAsync(acceptedSwapId);
        swap!.Status.Should().Be(SwapRequestStatus.Accepted);

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

        var handler = CreateRoleHandler(context);

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

        var handler = CreateRoleHandler(context);

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

        var handler = CreateRoleHandler(context);

        var act = async () => await handler.Handle(
            new UpdateUserRoleCommand { TargetUserId = targetId, NewRole = "Superadmin", RequestingUserId = managerId, OrganizationId = OrgId },
            CancellationToken.None);

        await act.Should().ThrowAsync<ArgumentException>()
            .WithMessage("*Ogiltig roll*");

        TestDbContextFactory.Destroy(context);
    }

    private static UpdateUserRoleCommandHandler CreateRoleHandler(Infrastructure.AppDbContext context)
    {
        var validatorMock = new Mock<IValidator<UpdateUserRoleCommand>>();
        validatorMock.Setup(v => v.ValidateAsync(It.IsAny<UpdateUserRoleCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult());
        return new UpdateUserRoleCommandHandler(context, validatorMock.Object);
    }

    private static void SeedOrg(Infrastructure.AppDbContext context)
    {
        context.Organizations.Add(new Organization { Id = OrgId, Name = "Test Org" });
        context.SaveChanges();
    }
}
