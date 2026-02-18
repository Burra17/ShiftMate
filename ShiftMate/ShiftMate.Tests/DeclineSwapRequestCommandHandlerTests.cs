using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using ShiftMate.Application.Interfaces;
using ShiftMate.Application.SwapRequests.Commands;
using ShiftMate.Domain;
using ShiftMate.Tests.Support;

namespace ShiftMate.Tests;

public class DeclineSwapRequestCommandHandlerTests
{
    [Fact]
    public async Task Handle_Should_Throw_When_SwapRequest_Not_Found()
    {
        // Arrange
        var context = TestDbContextFactory.Create();
        var handler = CreateHandler(context);

        var command = new DeclineSwapRequestCommand
        {
            SwapRequestId = Guid.NewGuid(),
            CurrentUserId = Guid.NewGuid()
        };

        // Act & Assert
        await FluentActions.Invoking(() => handler.Handle(command, CancellationToken.None))
            .Should().ThrowAsync<Exception>()
            .WithMessage("Bytesförfrågan kunde inte hittas.");

        TestDbContextFactory.Destroy(context);
    }

    [Fact]
    public async Task Handle_Should_Throw_When_User_Is_Not_Target()
    {
        // Arrange - användaren försöker neka en förfrågan som inte riktas till dem
        var context = TestDbContextFactory.Create();
        var requesterId = Guid.NewGuid();
        var targetId = Guid.NewGuid();
        var shiftId = Guid.NewGuid();
        var swapRequestId = Guid.NewGuid();

        context.Users.Add(new User
        {
            Id = requesterId, FirstName = "Requester", LastName = "R",
            Email = "req@test.com", PasswordHash = "hash", Role = Role.Employee
        });
        context.Users.Add(new User
        {
            Id = targetId, FirstName = "Target", LastName = "T",
            Email = "target@test.com", PasswordHash = "hash", Role = Role.Employee
        });
        context.Shifts.Add(new Shift
        {
            Id = shiftId, UserId = requesterId, IsUpForSwap = true,
            StartTime = DateTime.UtcNow.AddDays(1).Date.AddHours(8),
            EndTime = DateTime.UtcNow.AddDays(1).Date.AddHours(16)
        });
        context.SwapRequests.Add(new SwapRequest
        {
            Id = swapRequestId, ShiftId = shiftId, RequestingUserId = requesterId,
            TargetUserId = targetId, Status = SwapRequestStatus.Pending
        });
        await context.SaveChangesAsync(CancellationToken.None);

        var handler = CreateHandler(context);
        var command = new DeclineSwapRequestCommand
        {
            SwapRequestId = swapRequestId,
            CurrentUserId = Guid.NewGuid() // Annan användare, inte target
        };

        // Act & Assert
        await FluentActions.Invoking(() => handler.Handle(command, CancellationToken.None))
            .Should().ThrowAsync<Exception>()
            .WithMessage("Du har inte behörighet att neka denna förfrågan.");

        TestDbContextFactory.Destroy(context);
    }

    [Fact]
    public async Task Handle_Should_Throw_When_Request_Not_Pending()
    {
        // Arrange - förfrågan är redan hanterad
        var context = TestDbContextFactory.Create();
        var requesterId = Guid.NewGuid();
        var targetId = Guid.NewGuid();
        var shiftId = Guid.NewGuid();
        var swapRequestId = Guid.NewGuid();

        context.Users.Add(new User
        {
            Id = requesterId, FirstName = "Requester", LastName = "R",
            Email = "req@test.com", PasswordHash = "hash", Role = Role.Employee
        });
        context.Users.Add(new User
        {
            Id = targetId, FirstName = "Target", LastName = "T",
            Email = "target@test.com", PasswordHash = "hash", Role = Role.Employee
        });
        context.Shifts.Add(new Shift
        {
            Id = shiftId, UserId = requesterId, IsUpForSwap = true,
            StartTime = DateTime.UtcNow.AddDays(1).Date.AddHours(8),
            EndTime = DateTime.UtcNow.AddDays(1).Date.AddHours(16)
        });
        context.SwapRequests.Add(new SwapRequest
        {
            Id = swapRequestId, ShiftId = shiftId, RequestingUserId = requesterId,
            TargetUserId = targetId, Status = SwapRequestStatus.Accepted // Redan godkänd
        });
        await context.SaveChangesAsync(CancellationToken.None);

        var handler = CreateHandler(context);
        var command = new DeclineSwapRequestCommand
        {
            SwapRequestId = swapRequestId,
            CurrentUserId = targetId
        };

        // Act & Assert
        await FluentActions.Invoking(() => handler.Handle(command, CancellationToken.None))
            .Should().ThrowAsync<Exception>()
            .WithMessage("Denna förfrågan är inte längre aktiv och kan inte nekas.");

        TestDbContextFactory.Destroy(context);
    }

    [Fact]
    public async Task Handle_Should_Decline_SwapRequest_Successfully()
    {
        // Arrange - target-användaren nekar en pending-förfrågan
        var context = TestDbContextFactory.Create();
        var requesterId = Guid.NewGuid();
        var targetId = Guid.NewGuid();
        var shiftId = Guid.NewGuid();
        var swapRequestId = Guid.NewGuid();

        context.Users.Add(new User
        {
            Id = requesterId, FirstName = "Requester", LastName = "R",
            Email = "req@test.com", PasswordHash = "hash", Role = Role.Employee
        });
        context.Users.Add(new User
        {
            Id = targetId, FirstName = "Target", LastName = "T",
            Email = "target@test.com", PasswordHash = "hash", Role = Role.Employee
        });
        context.Shifts.Add(new Shift
        {
            Id = shiftId, UserId = requesterId, IsUpForSwap = true,
            StartTime = DateTime.UtcNow.AddDays(1).Date.AddHours(8),
            EndTime = DateTime.UtcNow.AddDays(1).Date.AddHours(16)
        });
        context.SwapRequests.Add(new SwapRequest
        {
            Id = swapRequestId, ShiftId = shiftId, RequestingUserId = requesterId,
            TargetUserId = targetId, Status = SwapRequestStatus.Pending
        });
        await context.SaveChangesAsync(CancellationToken.None);

        var handler = CreateHandler(context);
        var command = new DeclineSwapRequestCommand
        {
            SwapRequestId = swapRequestId,
            CurrentUserId = targetId
        };

        // Act
        await handler.Handle(command, CancellationToken.None);

        // Assert
        var updated = context.SwapRequests.First(sr => sr.Id == swapRequestId);
        updated.Status.Should().Be(SwapRequestStatus.Declined);

        TestDbContextFactory.Destroy(context);
    }

    private static DeclineSwapRequestCommandHandler CreateHandler(Infrastructure.AppDbContext context)
    {
        var mockEmailService = new Mock<IEmailService>();
        var mockLogger = new Mock<ILogger<DeclineSwapRequestCommandHandler>>();
        return new DeclineSwapRequestCommandHandler(context, mockEmailService.Object, mockLogger.Object);
    }
}
