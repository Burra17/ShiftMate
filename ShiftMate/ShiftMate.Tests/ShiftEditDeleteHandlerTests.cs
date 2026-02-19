using FluentAssertions;
using FluentValidation;
using FluentValidation.Results;
using Moq;
using ShiftMate.Application.Shifts.Commands;
using ShiftMate.Domain;
using ShiftMate.Tests.Support;
using Xunit;

namespace ShiftMate.Tests
{
    public class ShiftEditDeleteHandlerTests
    {
        // =====================================================================
        // UpdateShiftCommand — Tester
        // =====================================================================

        [Fact]
        public async Task UpdateShift_Should_Throw_When_Shift_Not_Found()
        {
            // ARRANGE — Försök uppdatera ett pass som inte finns
            var context = TestDbContextFactory.Create();

            var validatorMock = new Mock<IValidator<UpdateShiftCommand>>();
            validatorMock.Setup(v => v.ValidateAsync(It.IsAny<UpdateShiftCommand>(), It.IsAny<CancellationToken>()))
                         .ReturnsAsync(new ValidationResult());

            var handler = new UpdateShiftHandler(context, validatorMock.Object);

            var command = new UpdateShiftCommand
            {
                ShiftId = Guid.NewGuid(),
                StartTime = DateTime.UtcNow.AddHours(8),
                EndTime = DateTime.UtcNow.AddHours(16)
            };

            // ACT & ASSERT
            await FluentActions.Invoking(() => handler.Handle(command, CancellationToken.None))
                .Should().ThrowAsync<InvalidOperationException>()
                .WithMessage("Passet hittades inte.");

            TestDbContextFactory.Destroy(context);
        }

        [Fact]
        public async Task UpdateShift_Should_Update_Times_Successfully()
        {
            // ARRANGE — Skapa ett pass och uppdatera tiderna
            var context = TestDbContextFactory.Create();

            var shiftId = Guid.NewGuid();
            context.Shifts.Add(new Shift
            {
                Id = shiftId,
                StartTime = DateTime.UtcNow.AddHours(8),
                EndTime = DateTime.UtcNow.AddHours(16),
                UserId = null
            });
            await context.SaveChangesAsync(CancellationToken.None);

            var validatorMock = new Mock<IValidator<UpdateShiftCommand>>();
            validatorMock.Setup(v => v.ValidateAsync(It.IsAny<UpdateShiftCommand>(), It.IsAny<CancellationToken>()))
                         .ReturnsAsync(new ValidationResult());

            var handler = new UpdateShiftHandler(context, validatorMock.Object);

            var newStart = DateTime.UtcNow.AddHours(10);
            var newEnd = DateTime.UtcNow.AddHours(18);

            var command = new UpdateShiftCommand
            {
                ShiftId = shiftId,
                StartTime = newStart,
                EndTime = newEnd,
                UserId = null
            };

            // ACT
            var result = await handler.Handle(command, CancellationToken.None);

            // ASSERT
            result.Should().BeTrue();
            var updatedShift = context.Shifts.First(s => s.Id == shiftId);
            updatedShift.StartTime.Hour.Should().Be(newStart.Hour);
            updatedShift.EndTime.Hour.Should().Be(newEnd.Hour);

            TestDbContextFactory.Destroy(context);
        }

        [Fact]
        public async Task UpdateShift_Should_Throw_When_Overlap_Detected()
        {
            // ARRANGE — Skapa en användare med ett befintligt pass, försök flytta ett annat pass så det krockar
            var context = TestDbContextFactory.Create();

            var userId = Guid.NewGuid();
            context.Users.Add(new User
            {
                Id = userId,
                FirstName = "Test",
                LastName = "Testsson",
                Email = "test@test.com",
                PasswordHash = "hash",
                Role = Role.Employee
            });

            // Befintligt pass: 08:00 - 16:00
            context.Shifts.Add(new Shift
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                StartTime = DateTime.UtcNow.AddHours(8),
                EndTime = DateTime.UtcNow.AddHours(16)
            });

            // Passet vi vill uppdatera: 20:00 - 22:00 (krockar inte nu)
            var editShiftId = Guid.NewGuid();
            context.Shifts.Add(new Shift
            {
                Id = editShiftId,
                UserId = userId,
                StartTime = DateTime.UtcNow.AddHours(20),
                EndTime = DateTime.UtcNow.AddHours(22)
            });
            await context.SaveChangesAsync(CancellationToken.None);

            var validatorMock = new Mock<IValidator<UpdateShiftCommand>>();
            validatorMock.Setup(v => v.ValidateAsync(It.IsAny<UpdateShiftCommand>(), It.IsAny<CancellationToken>()))
                         .ReturnsAsync(new ValidationResult());

            var handler = new UpdateShiftHandler(context, validatorMock.Object);

            // Flytta det andra passet till 10:00 - 14:00 (krockar med 08-16!)
            var command = new UpdateShiftCommand
            {
                ShiftId = editShiftId,
                StartTime = DateTime.UtcNow.AddHours(10),
                EndTime = DateTime.UtcNow.AddHours(14),
                UserId = userId
            };

            // ACT & ASSERT
            await FluentActions.Invoking(() => handler.Handle(command, CancellationToken.None))
                .Should().ThrowAsync<InvalidOperationException>()
                .WithMessage("Denna användare har redan ett pass som krockar med den valda tiden.");

            TestDbContextFactory.Destroy(context);
        }

        [Fact]
        public async Task UpdateShift_Should_Not_Overlap_With_Self()
        {
            // ARRANGE — Uppdatera ett pass med samma tider ska inte ge krock-fel
            var context = TestDbContextFactory.Create();

            var userId = Guid.NewGuid();
            context.Users.Add(new User
            {
                Id = userId,
                FirstName = "Test",
                LastName = "Testsson",
                Email = "test@test.com",
                PasswordHash = "hash",
                Role = Role.Employee
            });

            var shiftId = Guid.NewGuid();
            context.Shifts.Add(new Shift
            {
                Id = shiftId,
                UserId = userId,
                StartTime = DateTime.UtcNow.AddHours(8),
                EndTime = DateTime.UtcNow.AddHours(16)
            });
            await context.SaveChangesAsync(CancellationToken.None);

            var validatorMock = new Mock<IValidator<UpdateShiftCommand>>();
            validatorMock.Setup(v => v.ValidateAsync(It.IsAny<UpdateShiftCommand>(), It.IsAny<CancellationToken>()))
                         .ReturnsAsync(new ValidationResult());

            var handler = new UpdateShiftHandler(context, validatorMock.Object);

            // Uppdatera samma pass med liknande tider (exkluderar sig själv)
            var command = new UpdateShiftCommand
            {
                ShiftId = shiftId,
                StartTime = DateTime.UtcNow.AddHours(9),
                EndTime = DateTime.UtcNow.AddHours(17),
                UserId = userId
            };

            // ACT
            var result = await handler.Handle(command, CancellationToken.None);

            // ASSERT — Ska lyckas utan krock (exkluderar eget pass)
            result.Should().BeTrue();

            TestDbContextFactory.Destroy(context);
        }

        // =====================================================================
        // DeleteShiftCommand — Tester
        // =====================================================================

        [Fact]
        public async Task DeleteShift_Should_Throw_When_Shift_Not_Found()
        {
            // ARRANGE
            var context = TestDbContextFactory.Create();
            var handler = new DeleteShiftHandler(context);

            // ACT & ASSERT
            await FluentActions.Invoking(() => handler.Handle(new DeleteShiftCommand(Guid.NewGuid()), CancellationToken.None))
                .Should().ThrowAsync<InvalidOperationException>()
                .WithMessage("Passet hittades inte.");

            TestDbContextFactory.Destroy(context);
        }

        [Fact]
        public async Task DeleteShift_Should_Remove_Shift_Successfully()
        {
            // ARRANGE — Skapa ett pass och radera det
            var context = TestDbContextFactory.Create();

            var shiftId = Guid.NewGuid();
            context.Shifts.Add(new Shift
            {
                Id = shiftId,
                StartTime = DateTime.UtcNow.AddHours(8),
                EndTime = DateTime.UtcNow.AddHours(16),
                UserId = null
            });
            await context.SaveChangesAsync(CancellationToken.None);

            var handler = new DeleteShiftHandler(context);

            // ACT
            var result = await handler.Handle(new DeleteShiftCommand(shiftId), CancellationToken.None);

            // ASSERT
            result.Should().BeTrue();
            context.Shifts.Should().HaveCount(0);

            TestDbContextFactory.Destroy(context);
        }

        [Fact]
        public async Task DeleteShift_Should_Cancel_Pending_SwapRequests()
        {
            // ARRANGE — Skapa ett pass med en väntande bytesförfrågan
            var context = TestDbContextFactory.Create();

            var userId = Guid.NewGuid();
            context.Users.Add(new User
            {
                Id = userId,
                FirstName = "Test",
                LastName = "Testsson",
                Email = "test@test.com",
                PasswordHash = "hash",
                Role = Role.Employee
            });

            var requestingUserId = Guid.NewGuid();
            context.Users.Add(new User
            {
                Id = requestingUserId,
                FirstName = "Req",
                LastName = "Reqsson",
                Email = "req@test.com",
                PasswordHash = "hash",
                Role = Role.Employee
            });

            var shiftId = Guid.NewGuid();
            context.Shifts.Add(new Shift
            {
                Id = shiftId,
                StartTime = DateTime.UtcNow.AddHours(8),
                EndTime = DateTime.UtcNow.AddHours(16),
                UserId = userId,
                IsUpForSwap = true
            });

            var swapRequestId = Guid.NewGuid();
            context.SwapRequests.Add(new SwapRequest
            {
                Id = swapRequestId,
                ShiftId = shiftId,
                RequestingUserId = requestingUserId,
                Status = SwapRequestStatus.Pending,
                CreatedAt = DateTime.UtcNow
            });
            await context.SaveChangesAsync(CancellationToken.None);

            var handler = new DeleteShiftHandler(context);

            // ACT
            var result = await handler.Handle(new DeleteShiftCommand(shiftId), CancellationToken.None);

            // ASSERT — Passet borttaget, bytesförfrågningar borttagna
            result.Should().BeTrue();
            context.Shifts.Should().HaveCount(0);
            context.SwapRequests.Should().HaveCount(0);

            TestDbContextFactory.Destroy(context);
        }
    }
}
