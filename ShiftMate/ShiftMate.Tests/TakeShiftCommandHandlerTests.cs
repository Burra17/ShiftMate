using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using ShiftMate.Application.Interfaces;
using ShiftMate.Application.Shifts.Commands;
using ShiftMate.Domain;
using ShiftMate.Tests.Support;

namespace ShiftMate.Tests;

public class TakeShiftCommandHandlerTests
{
    private static readonly Guid OrgId = Guid.NewGuid();

    [Fact]
    public async Task Handle_Should_Throw_When_Shift_Not_Found()
    {
        var context = TestDbContextFactory.Create();
        var handler = CreateHandler(context);

        var command = new TakeShiftCommand
        {
            ShiftId = Guid.NewGuid(),
            UserId = Guid.NewGuid(),
            OrganizationId = OrgId
        };

        await FluentActions.Invoking(() => handler.Handle(command, CancellationToken.None))
            .Should().ThrowAsync<Exception>()
            .WithMessage("Arbetspasset kunde inte hittas.");

        TestDbContextFactory.Destroy(context);
    }

    [Fact]
    public async Task Handle_Should_Throw_When_Shift_Not_Available()
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
            Id = shiftId, UserId = ownerId, IsUpForSwap = false, OrganizationId = OrgId,
            StartTime = DateTime.UtcNow.AddDays(1).Date.AddHours(8),
            EndTime = DateTime.UtcNow.AddDays(1).Date.AddHours(16)
        });
        await context.SaveChangesAsync(CancellationToken.None);

        var handler = CreateHandler(context);
        var command = new TakeShiftCommand { ShiftId = shiftId, UserId = Guid.NewGuid(), OrganizationId = OrgId };

        await FluentActions.Invoking(() => handler.Handle(command, CancellationToken.None))
            .Should().ThrowAsync<Exception>()
            .WithMessage("Detta pass är inte tillgängligt för att tas.");

        TestDbContextFactory.Destroy(context);
    }

    [Fact]
    public async Task Handle_Should_Throw_When_User_Not_Found()
    {
        var context = TestDbContextFactory.Create();
        SeedOrg(context);
        var shiftId = Guid.NewGuid();

        context.Shifts.Add(new Shift
        {
            Id = shiftId, UserId = null, IsUpForSwap = false, OrganizationId = OrgId,
            StartTime = DateTime.UtcNow.AddDays(1).Date.AddHours(8),
            EndTime = DateTime.UtcNow.AddDays(1).Date.AddHours(16)
        });
        await context.SaveChangesAsync(CancellationToken.None);

        var handler = CreateHandler(context);
        var command = new TakeShiftCommand { ShiftId = shiftId, UserId = Guid.NewGuid(), OrganizationId = OrgId };

        await FluentActions.Invoking(() => handler.Handle(command, CancellationToken.None))
            .Should().ThrowAsync<Exception>()
            .WithMessage("Användaren kunde inte hittas.");

        TestDbContextFactory.Destroy(context);
    }

    [Fact]
    public async Task Handle_Should_Throw_When_User_Has_Shift_On_Same_Day()
    {
        var context = TestDbContextFactory.Create();
        SeedOrg(context);
        var userId = Guid.NewGuid();
        var shiftId = Guid.NewGuid();
        var tomorrow = DateTime.UtcNow.AddDays(1).Date;

        context.Users.Add(new User
        {
            Id = userId, FirstName = "Test", LastName = "Testsson",
            Email = "test@test.com", PasswordHash = "hash", Role = Role.Employee, OrganizationId = OrgId
        });
        context.Shifts.Add(new Shift
        {
            Id = Guid.NewGuid(), UserId = userId, IsUpForSwap = false, OrganizationId = OrgId,
            StartTime = tomorrow.AddHours(8), EndTime = tomorrow.AddHours(12)
        });
        context.Shifts.Add(new Shift
        {
            Id = shiftId, UserId = null, IsUpForSwap = false, OrganizationId = OrgId,
            StartTime = tomorrow.AddHours(14), EndTime = tomorrow.AddHours(18)
        });
        await context.SaveChangesAsync(CancellationToken.None);

        var handler = CreateHandler(context);
        var command = new TakeShiftCommand { ShiftId = shiftId, UserId = userId, OrganizationId = OrgId };

        await FluentActions.Invoking(() => handler.Handle(command, CancellationToken.None))
            .Should().ThrowAsync<Exception>()
            .WithMessage("Du kan inte ta ett pass på en dag där du redan har ett annat pass.");

        TestDbContextFactory.Destroy(context);
    }

    [Fact]
    public async Task Handle_Should_Assign_Open_Shift_To_User()
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
            Id = shiftId, UserId = null, IsUpForSwap = false, OrganizationId = OrgId,
            StartTime = DateTime.UtcNow.AddDays(1).Date.AddHours(8),
            EndTime = DateTime.UtcNow.AddDays(1).Date.AddHours(16)
        });
        await context.SaveChangesAsync(CancellationToken.None);

        var handler = CreateHandler(context);
        var command = new TakeShiftCommand { ShiftId = shiftId, UserId = userId, OrganizationId = OrgId };

        var result = await handler.Handle(command, CancellationToken.None);

        result.Should().BeTrue();
        var updatedShift = context.Shifts.First(s => s.Id == shiftId);
        updatedShift.UserId.Should().Be(userId);
        updatedShift.IsUpForSwap.Should().BeFalse();

        TestDbContextFactory.Destroy(context);
    }

    private static TakeShiftCommandHandler CreateHandler(Infrastructure.AppDbContext context)
    {
        var mockEmailService = new Mock<IEmailService>();
        var mockLogger = new Mock<ILogger<TakeShiftCommandHandler>>();
        return new TakeShiftCommandHandler(context, mockEmailService.Object, mockLogger.Object);
    }

    private static void SeedOrg(Infrastructure.AppDbContext context)
    {
        context.Organizations.Add(new Organization { Id = OrgId, Name = "Test Org" });
        context.SaveChanges();
    }
}
