using FluentAssertions;
using ShiftMate.Application.Shifts.Commands;
using ShiftMate.Domain;
using ShiftMate.Tests.Support;

namespace ShiftMate.Tests;

public class CancelShiftSwapCommandHandlerTests
{
    private static readonly Guid OrgId = Guid.NewGuid();

    [Fact]
    public async Task Handle_Should_Throw_When_Shift_Not_Found()
    {
        var context = TestDbContextFactory.Create();
        var handler = new CancelShiftSwapCommandHandler(context);

        var command = new CancelShiftSwapCommand
        {
            ShiftId = Guid.NewGuid(),
            UserId = Guid.NewGuid()
        };

        await FluentActions.Invoking(() => handler.Handle(command, CancellationToken.None))
            .Should().ThrowAsync<Exception>()
            .WithMessage("Arbetspasset kunde inte hittas.");

        TestDbContextFactory.Destroy(context);
    }

    [Fact]
    public async Task Handle_Should_Throw_When_User_Does_Not_Own_Shift()
    {
        var context = TestDbContextFactory.Create();
        SeedOrg(context);
        var ownerId = Guid.NewGuid();
        var shiftId = Guid.NewGuid();

        context.Users.Add(new User
        {
            Id = ownerId, FirstName = "Owner", LastName = "Ownersson",
            Email = "owner@test.com", PasswordHash = "hash", Role = Role.Employee, OrganizationId = OrgId
        });
        context.Shifts.Add(new Shift
        {
            Id = shiftId, UserId = ownerId, IsUpForSwap = true, OrganizationId = OrgId,
            StartTime = DateTime.UtcNow.AddDays(1).Date.AddHours(8),
            EndTime = DateTime.UtcNow.AddDays(1).Date.AddHours(16)
        });
        await context.SaveChangesAsync(CancellationToken.None);

        var handler = new CancelShiftSwapCommandHandler(context);
        var command = new CancelShiftSwapCommand
        {
            ShiftId = shiftId,
            UserId = Guid.NewGuid()
        };

        await FluentActions.Invoking(() => handler.Handle(command, CancellationToken.None))
            .Should().ThrowAsync<Exception>()
            .WithMessage("Du kan inte ångra ett pass som inte är ditt.");

        TestDbContextFactory.Destroy(context);
    }

    [Fact]
    public async Task Handle_Should_Throw_When_Shift_Not_Marked_For_Swap()
    {
        var context = TestDbContextFactory.Create();
        SeedOrg(context);
        var userId = Guid.NewGuid();
        var shiftId = Guid.NewGuid();

        context.Users.Add(new User
        {
            Id = userId, FirstName = "Test", LastName = "Testsson",
            Email = "test@test.com", PasswordHash = "hash", Role = Role.Employee, OrganizationId = OrgId
        });
        context.Shifts.Add(new Shift
        {
            Id = shiftId, UserId = userId, IsUpForSwap = false, OrganizationId = OrgId,
            StartTime = DateTime.UtcNow.AddDays(1).Date.AddHours(8),
            EndTime = DateTime.UtcNow.AddDays(1).Date.AddHours(16)
        });
        await context.SaveChangesAsync(CancellationToken.None);

        var handler = new CancelShiftSwapCommandHandler(context);
        var command = new CancelShiftSwapCommand { ShiftId = shiftId, UserId = userId };

        await FluentActions.Invoking(() => handler.Handle(command, CancellationToken.None))
            .Should().ThrowAsync<Exception>()
            .WithMessage("Detta pass är inte markerat som ledigt för byte.");

        TestDbContextFactory.Destroy(context);
    }

    [Fact]
    public async Task Handle_Should_Cancel_Swap_Successfully()
    {
        var context = TestDbContextFactory.Create();
        SeedOrg(context);
        var userId = Guid.NewGuid();
        var shiftId = Guid.NewGuid();

        context.Users.Add(new User
        {
            Id = userId, FirstName = "Test", LastName = "Testsson",
            Email = "test@test.com", PasswordHash = "hash", Role = Role.Employee, OrganizationId = OrgId
        });
        context.Shifts.Add(new Shift
        {
            Id = shiftId, UserId = userId, IsUpForSwap = true, OrganizationId = OrgId,
            StartTime = DateTime.UtcNow.AddDays(1).Date.AddHours(8),
            EndTime = DateTime.UtcNow.AddDays(1).Date.AddHours(16)
        });
        await context.SaveChangesAsync(CancellationToken.None);

        var handler = new CancelShiftSwapCommandHandler(context);
        var command = new CancelShiftSwapCommand { ShiftId = shiftId, UserId = userId };

        var result = await handler.Handle(command, CancellationToken.None);

        result.Should().BeTrue();
        var updatedShift = context.Shifts.First(s => s.Id == shiftId);
        updatedShift.IsUpForSwap.Should().BeFalse();

        TestDbContextFactory.Destroy(context);
    }

    private static void SeedOrg(Infrastructure.AppDbContext context)
    {
        context.Organizations.Add(new Organization { Id = OrgId, Name = "Test Org" });
        context.SaveChanges();
    }
}
