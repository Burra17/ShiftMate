using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using ShiftMate.Application.Interfaces;
using ShiftMate.Application.SwapRequests.Commands;
using ShiftMate.Domain;
using ShiftMate.Tests.Support;

namespace ShiftMate.Tests;

public class ProposeDirectSwapCommandHandlerTests
{
    [Fact]
    public async Task Handle_Should_Throw_When_Shifts_Not_Found()
    {
        // Arrange - passen finns inte
        var context = TestDbContextFactory.Create();
        var handler = CreateHandler(context);

        var command = new ProposeDirectSwapCommand
        {
            MyShiftId = Guid.NewGuid(),
            TargetShiftId = Guid.NewGuid(),
            RequestingUserId = Guid.NewGuid()
        };

        // Act & Assert
        await FluentActions.Invoking(() => handler.Handle(command, CancellationToken.None))
            .Should().ThrowAsync<Exception>()
            .WithMessage("Kunde inte hitta passen eller målanvändaren. Passet kan sakna ägare.");

        TestDbContextFactory.Destroy(context);
    }

    [Fact]
    public async Task Handle_Should_Throw_When_User_Does_Not_Own_Shift()
    {
        // Arrange - användaren äger inte passet de vill byta bort
        var context = TestDbContextFactory.Create();
        var requesterId = Guid.NewGuid();
        var ownerId = Guid.NewGuid();
        var targetOwnerId = Guid.NewGuid();
        var myShiftId = Guid.NewGuid();
        var targetShiftId = Guid.NewGuid();

        context.Users.Add(new User
        {
            Id = ownerId, FirstName = "Owner", LastName = "O",
            Email = "owner@test.com", PasswordHash = "hash", Role = Role.Employee
        });
        context.Users.Add(new User
        {
            Id = targetOwnerId, FirstName = "Target", LastName = "T",
            Email = "target@test.com", PasswordHash = "hash", Role = Role.Employee
        });
        context.Users.Add(new User
        {
            Id = requesterId, FirstName = "Requester", LastName = "R",
            Email = "req@test.com", PasswordHash = "hash", Role = Role.Employee
        });
        context.Shifts.Add(new Shift
        {
            Id = myShiftId, UserId = ownerId, IsUpForSwap = false,
            StartTime = DateTime.UtcNow.AddDays(1).Date.AddHours(8),
            EndTime = DateTime.UtcNow.AddDays(1).Date.AddHours(16)
        });
        context.Shifts.Add(new Shift
        {
            Id = targetShiftId, UserId = targetOwnerId, IsUpForSwap = false,
            StartTime = DateTime.UtcNow.AddDays(2).Date.AddHours(8),
            EndTime = DateTime.UtcNow.AddDays(2).Date.AddHours(16)
        });
        await context.SaveChangesAsync(CancellationToken.None);

        var handler = CreateHandler(context);
        var command = new ProposeDirectSwapCommand
        {
            MyShiftId = myShiftId,
            TargetShiftId = targetShiftId,
            RequestingUserId = requesterId // Inte ägaren av myShift
        };

        // Act & Assert
        await FluentActions.Invoking(() => handler.Handle(command, CancellationToken.None))
            .Should().ThrowAsync<Exception>()
            .WithMessage("Du kan bara föreslå byte för pass du själv äger.");

        TestDbContextFactory.Destroy(context);
    }

    [Fact]
    public async Task Handle_Should_Throw_When_Target_Shift_Has_No_Owner()
    {
        // Arrange - målpasset saknar ägare (filtreras bort i query)
        var context = TestDbContextFactory.Create();
        var requesterId = Guid.NewGuid();
        var myShiftId = Guid.NewGuid();
        var targetShiftId = Guid.NewGuid();

        context.Users.Add(new User
        {
            Id = requesterId, FirstName = "Requester", LastName = "R",
            Email = "req@test.com", PasswordHash = "hash", Role = Role.Employee
        });
        context.Shifts.Add(new Shift
        {
            Id = myShiftId, UserId = requesterId, IsUpForSwap = false,
            StartTime = DateTime.UtcNow.AddDays(1).Date.AddHours(8),
            EndTime = DateTime.UtcNow.AddDays(1).Date.AddHours(16)
        });
        context.Shifts.Add(new Shift
        {
            Id = targetShiftId, UserId = null, IsUpForSwap = false, // Inget ägarskap
            StartTime = DateTime.UtcNow.AddDays(2).Date.AddHours(8),
            EndTime = DateTime.UtcNow.AddDays(2).Date.AddHours(16)
        });
        await context.SaveChangesAsync(CancellationToken.None);

        var handler = CreateHandler(context);
        var command = new ProposeDirectSwapCommand
        {
            MyShiftId = myShiftId,
            TargetShiftId = targetShiftId,
            RequestingUserId = requesterId
        };

        // Act & Assert
        await FluentActions.Invoking(() => handler.Handle(command, CancellationToken.None))
            .Should().ThrowAsync<Exception>()
            .WithMessage("Kunde inte hitta passen eller målanvändaren. Passet kan sakna ägare.");

        TestDbContextFactory.Destroy(context);
    }

    [Fact]
    public async Task Handle_Should_Create_SwapRequest_Successfully()
    {
        // Arrange - lyckad direkt bytesförfrågan
        var context = TestDbContextFactory.Create();
        var requesterId = Guid.NewGuid();
        var targetOwnerId = Guid.NewGuid();
        var myShiftId = Guid.NewGuid();
        var targetShiftId = Guid.NewGuid();

        context.Users.Add(new User
        {
            Id = requesterId, FirstName = "Requester", LastName = "R",
            Email = "req@test.com", PasswordHash = "hash", Role = Role.Employee
        });
        context.Users.Add(new User
        {
            Id = targetOwnerId, FirstName = "Target", LastName = "T",
            Email = "target@test.com", PasswordHash = "hash", Role = Role.Employee
        });
        context.Shifts.Add(new Shift
        {
            Id = myShiftId, UserId = requesterId, IsUpForSwap = false,
            StartTime = DateTime.UtcNow.AddDays(1).Date.AddHours(8),
            EndTime = DateTime.UtcNow.AddDays(1).Date.AddHours(16)
        });
        context.Shifts.Add(new Shift
        {
            Id = targetShiftId, UserId = targetOwnerId, IsUpForSwap = false,
            StartTime = DateTime.UtcNow.AddDays(2).Date.AddHours(8),
            EndTime = DateTime.UtcNow.AddDays(2).Date.AddHours(16)
        });
        await context.SaveChangesAsync(CancellationToken.None);

        var handler = CreateHandler(context);
        var command = new ProposeDirectSwapCommand
        {
            MyShiftId = myShiftId,
            TargetShiftId = targetShiftId,
            RequestingUserId = requesterId
        };

        // Act
        var resultId = await handler.Handle(command, CancellationToken.None);

        // Assert
        resultId.Should().NotBeEmpty();
        context.SwapRequests.Should().HaveCount(1);

        var swapRequest = context.SwapRequests.First();
        swapRequest.ShiftId.Should().Be(myShiftId);
        swapRequest.RequestingUserId.Should().Be(requesterId);
        swapRequest.TargetUserId.Should().Be(targetOwnerId);
        swapRequest.TargetShiftId.Should().Be(targetShiftId);
        swapRequest.Status.Should().Be("Pending");

        TestDbContextFactory.Destroy(context);
    }

    private static ProposeDirectSwapCommandHandler CreateHandler(Infrastructure.AppDbContext context)
    {
        var mockEmailService = new Mock<IEmailService>();
        var mockLogger = new Mock<ILogger<ProposeDirectSwapCommandHandler>>();
        return new ProposeDirectSwapCommandHandler(context, mockEmailService.Object, mockLogger.Object);
    }
}
