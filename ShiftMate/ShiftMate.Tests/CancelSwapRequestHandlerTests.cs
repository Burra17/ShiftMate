using FluentAssertions;
using ShiftMate.Application.SwapRequests.Commands;
using ShiftMate.Domain;
using ShiftMate.Tests.Support;

namespace ShiftMate.Tests;

public class CancelSwapRequestHandlerTests
{
    private static readonly Guid OrgId = Guid.NewGuid();

    [Fact]
    public async Task Handle_Should_Throw_When_SwapRequest_Not_Found()
    {
        var context = TestDbContextFactory.Create();
        var handler = new CancelSwapRequestHandler(context);

        var command = new CancelSwapRequestCommand(Guid.NewGuid(), Guid.NewGuid());

        await FluentActions.Invoking(() => handler.Handle(command, CancellationToken.None))
            .Should().ThrowAsync<Exception>()
            .WithMessage("Hittade inte bytesförfrågan.");

        TestDbContextFactory.Destroy(context);
    }

    [Fact]
    public async Task Handle_Should_Throw_When_User_Is_Not_Requester()
    {
        var context = TestDbContextFactory.Create();
        SeedOrg(context);
        var requesterId = Guid.NewGuid();
        var shiftId = Guid.NewGuid();
        var swapRequestId = Guid.NewGuid();

        context.Users.Add(new User
        {
            Id = requesterId, FirstName = "Requester", LastName = "R",
            Email = "req@test.com", PasswordHash = "hash", Role = Role.Employee, OrganizationId = OrgId
        });
        context.Shifts.Add(new Shift
        {
            Id = shiftId, UserId = requesterId, IsUpForSwap = true, OrganizationId = OrgId,
            StartTime = DateTime.UtcNow.AddDays(1).Date.AddHours(8),
            EndTime = DateTime.UtcNow.AddDays(1).Date.AddHours(16)
        });
        context.SwapRequests.Add(new SwapRequest
        {
            Id = swapRequestId, ShiftId = shiftId, RequestingUserId = requesterId,
            Status = SwapRequestStatus.Pending
        });
        await context.SaveChangesAsync(CancellationToken.None);

        var handler = new CancelSwapRequestHandler(context);
        var command = new CancelSwapRequestCommand(swapRequestId, Guid.NewGuid());

        await FluentActions.Invoking(() => handler.Handle(command, CancellationToken.None))
            .Should().ThrowAsync<Exception>()
            .WithMessage("Du får inte ta bort någon annans bytesförfrågan!");

        TestDbContextFactory.Destroy(context);
    }

    [Fact]
    public async Task Handle_Should_Delete_SwapRequest_And_Reset_Shift()
    {
        var context = TestDbContextFactory.Create();
        SeedOrg(context);
        var requesterId = Guid.NewGuid();
        var shiftId = Guid.NewGuid();
        var swapRequestId = Guid.NewGuid();

        context.Users.Add(new User
        {
            Id = requesterId, FirstName = "Requester", LastName = "R",
            Email = "req@test.com", PasswordHash = "hash", Role = Role.Employee, OrganizationId = OrgId
        });
        context.Shifts.Add(new Shift
        {
            Id = shiftId, UserId = requesterId, IsUpForSwap = true, OrganizationId = OrgId,
            StartTime = DateTime.UtcNow.AddDays(1).Date.AddHours(8),
            EndTime = DateTime.UtcNow.AddDays(1).Date.AddHours(16)
        });
        context.SwapRequests.Add(new SwapRequest
        {
            Id = swapRequestId, ShiftId = shiftId, RequestingUserId = requesterId,
            Status = SwapRequestStatus.Pending
        });
        await context.SaveChangesAsync(CancellationToken.None);

        var handler = new CancelSwapRequestHandler(context);
        var command = new CancelSwapRequestCommand(swapRequestId, requesterId);

        await handler.Handle(command, CancellationToken.None);

        context.SwapRequests.Should().BeEmpty();
        var shift = context.Shifts.First(s => s.Id == shiftId);
        shift.IsUpForSwap.Should().BeFalse();

        TestDbContextFactory.Destroy(context);
    }

    private static void SeedOrg(Infrastructure.AppDbContext context)
    {
        context.Organizations.Add(new Organization { Id = OrgId, Name = "Test Org" });
        context.SaveChanges();
    }
}
