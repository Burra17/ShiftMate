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
    [Fact]
    public async Task Handle_Should_Throw_When_Shift_Not_Found()
    {
        // Arrange
        var context = TestDbContextFactory.Create();
        var handler = CreateHandler(context);

        var command = new TakeShiftCommand
        {
            ShiftId = Guid.NewGuid(),
            UserId = Guid.NewGuid()
        };

        // Act & Assert
        await FluentActions.Invoking(() => handler.Handle(command, CancellationToken.None))
            .Should().ThrowAsync<Exception>()
            .WithMessage("Arbetspasset kunde inte hittas.");

        TestDbContextFactory.Destroy(context);
    }

    [Fact]
    public async Task Handle_Should_Throw_When_Shift_Not_Available()
    {
        // Arrange - pass som redan tillhör en annan användare och inte är markerat för byte
        var context = TestDbContextFactory.Create();
        var ownerId = Guid.NewGuid();
        var shiftId = Guid.NewGuid();

        context.Users.Add(new User
        {
            Id = ownerId, FirstName = "Owner", LastName = "Ownersson",
            Email = "owner@test.com", PasswordHash = "hash", Role = Role.Employee
        });
        context.Shifts.Add(new Shift
        {
            Id = shiftId, UserId = ownerId, IsUpForSwap = false,
            StartTime = DateTime.UtcNow.AddDays(1).Date.AddHours(8),
            EndTime = DateTime.UtcNow.AddDays(1).Date.AddHours(16)
        });
        await context.SaveChangesAsync(CancellationToken.None);

        var handler = CreateHandler(context);
        var command = new TakeShiftCommand { ShiftId = shiftId, UserId = Guid.NewGuid() };

        // Act & Assert
        await FluentActions.Invoking(() => handler.Handle(command, CancellationToken.None))
            .Should().ThrowAsync<Exception>()
            .WithMessage("Detta pass är inte tillgängligt för att tas.");

        TestDbContextFactory.Destroy(context);
    }

    [Fact]
    public async Task Handle_Should_Throw_When_User_Not_Found()
    {
        // Arrange - öppet pass utan ägare, men användaren finns inte
        var context = TestDbContextFactory.Create();
        var shiftId = Guid.NewGuid();

        context.Shifts.Add(new Shift
        {
            Id = shiftId, UserId = null, IsUpForSwap = false,
            StartTime = DateTime.UtcNow.AddDays(1).Date.AddHours(8),
            EndTime = DateTime.UtcNow.AddDays(1).Date.AddHours(16)
        });
        await context.SaveChangesAsync(CancellationToken.None);

        var handler = CreateHandler(context);
        var command = new TakeShiftCommand { ShiftId = shiftId, UserId = Guid.NewGuid() };

        // Act & Assert
        await FluentActions.Invoking(() => handler.Handle(command, CancellationToken.None))
            .Should().ThrowAsync<Exception>()
            .WithMessage("Användaren kunde inte hittas.");

        TestDbContextFactory.Destroy(context);
    }

    [Fact]
    public async Task Handle_Should_Throw_When_User_Has_Shift_On_Same_Day()
    {
        // Arrange - användaren har redan ett pass samma dag
        var context = TestDbContextFactory.Create();
        var userId = Guid.NewGuid();
        var shiftId = Guid.NewGuid();
        var tomorrow = DateTime.UtcNow.AddDays(1).Date;

        context.Users.Add(new User
        {
            Id = userId, FirstName = "Test", LastName = "Testsson",
            Email = "test@test.com", PasswordHash = "hash", Role = Role.Employee
        });
        // Befintligt pass samma dag
        context.Shifts.Add(new Shift
        {
            Id = Guid.NewGuid(), UserId = userId, IsUpForSwap = false,
            StartTime = tomorrow.AddHours(8), EndTime = tomorrow.AddHours(12)
        });
        // Pass att ta - öppet pass samma dag
        context.Shifts.Add(new Shift
        {
            Id = shiftId, UserId = null, IsUpForSwap = false,
            StartTime = tomorrow.AddHours(14), EndTime = tomorrow.AddHours(18)
        });
        await context.SaveChangesAsync(CancellationToken.None);

        var handler = CreateHandler(context);
        var command = new TakeShiftCommand { ShiftId = shiftId, UserId = userId };

        // Act & Assert
        await FluentActions.Invoking(() => handler.Handle(command, CancellationToken.None))
            .Should().ThrowAsync<Exception>()
            .WithMessage("Du kan inte ta ett pass på en dag där du redan har ett annat pass.");

        TestDbContextFactory.Destroy(context);
    }

    [Fact]
    public async Task Handle_Should_Assign_Open_Shift_To_User()
    {
        // Arrange - öppet pass utan ägare, användaren tar det
        var context = TestDbContextFactory.Create();
        var userId = Guid.NewGuid();
        var shiftId = Guid.NewGuid();

        context.Users.Add(new User
        {
            Id = userId, FirstName = "Test", LastName = "Testsson",
            Email = "test@test.com", PasswordHash = "hash", Role = Role.Employee
        });
        context.Shifts.Add(new Shift
        {
            Id = shiftId, UserId = null, IsUpForSwap = false,
            StartTime = DateTime.UtcNow.AddDays(1).Date.AddHours(8),
            EndTime = DateTime.UtcNow.AddDays(1).Date.AddHours(16)
        });
        await context.SaveChangesAsync(CancellationToken.None);

        var handler = CreateHandler(context);
        var command = new TakeShiftCommand { ShiftId = shiftId, UserId = userId };

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
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
}
