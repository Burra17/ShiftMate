using FluentAssertions;
using FluentValidation;
using FluentValidation.Results;
using Moq;
using ShiftMate.Application.Shifts.Commands;
using ShiftMate.Domain;
using ShiftMate.Tests.Support;
using Xunit;
using ShiftMate.Application.Interfaces;
using Microsoft.Extensions.Logging;

namespace ShiftMate.Tests
{
    public class CreateShiftHandlerTests
    {
        [Fact]
        public async Task Handle_Should_Save_Shift_To_Database()
        {
            // 1. ARRANGE (Förbered)
            var context = TestDbContextFactory.Create();

            // Skapa en användare i databasen (handlern kontrollerar att användaren finns)
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
            await context.SaveChangesAsync(CancellationToken.None);

            // Skapa en mock-validator som alltid godkänner
            var validatorMock = new Mock<IValidator<CreateShiftCommand>>();
            validatorMock.Setup(v => v.ValidateAsync(It.IsAny<CreateShiftCommand>(), It.IsAny<CancellationToken>()))
                         .ReturnsAsync(new ValidationResult());

            var mockEmailService = new Mock<IEmailService>();
            var mockLogger = new Mock<ILogger<CreateShiftHandler>>();
            var handler = new CreateShiftHandler(context, validatorMock.Object, mockEmailService.Object, mockLogger.Object);

            var command = new CreateShiftCommand
            {
                UserId = userId,
                StartTime = DateTime.UtcNow.AddHours(8),
                EndTime = DateTime.UtcNow.AddHours(16)
            };

            // 2. ACT (Utför)
            var resultId = await handler.Handle(command, CancellationToken.None);

            // 3. ASSERT (Kontrollera)
            resultId.Should().NotBeEmpty();
            context.Shifts.Should().HaveCount(1);
            context.Shifts.First().UserId.Should().Be(userId);

            // Städa upp
            TestDbContextFactory.Destroy(context);
        }

        [Fact]
        public async Task Handle_Should_Create_Open_Shift_Without_UserId()
        {
            // ARRANGE — Skapa ett "öppet pass" utan tilldelad användare
            var context = TestDbContextFactory.Create();

            var validatorMock = new Mock<IValidator<CreateShiftCommand>>();
            validatorMock.Setup(v => v.ValidateAsync(It.IsAny<CreateShiftCommand>(), It.IsAny<CancellationToken>()))
                         .ReturnsAsync(new ValidationResult());

            var mockEmailService = new Mock<IEmailService>();
            var mockLogger = new Mock<ILogger<CreateShiftHandler>>();
            var handler = new CreateShiftHandler(context, validatorMock.Object, mockEmailService.Object, mockLogger.Object);

            var command = new CreateShiftCommand
            {
                UserId = null,
                StartTime = DateTime.UtcNow.AddHours(8),
                EndTime = DateTime.UtcNow.AddHours(16)
            };

            // ACT
            var resultId = await handler.Handle(command, CancellationToken.None);

            // ASSERT — Passet ska skapas utan ägare
            resultId.Should().NotBeEmpty();
            context.Shifts.Should().HaveCount(1);
            context.Shifts.First().UserId.Should().BeNull();

            TestDbContextFactory.Destroy(context);
        }

        [Fact]
        public async Task Handle_Should_Throw_When_User_Not_Found()
        {
            // ARRANGE — Skicka ett UserId som inte finns i databasen
            var context = TestDbContextFactory.Create();

            var validatorMock = new Mock<IValidator<CreateShiftCommand>>();
            validatorMock.Setup(v => v.ValidateAsync(It.IsAny<CreateShiftCommand>(), It.IsAny<CancellationToken>()))
                         .ReturnsAsync(new ValidationResult());

            var mockEmailService = new Mock<IEmailService>();
            var mockLogger = new Mock<ILogger<CreateShiftHandler>>();
            var handler = new CreateShiftHandler(context, validatorMock.Object, mockEmailService.Object, mockLogger.Object);

            var command = new CreateShiftCommand
            {
                UserId = Guid.NewGuid(),
                StartTime = DateTime.UtcNow.AddHours(8),
                EndTime = DateTime.UtcNow.AddHours(16)
            };

            // ACT & ASSERT — Ska kasta undantag om användaren inte finns
            await FluentActions.Invoking(() => handler.Handle(command, CancellationToken.None))
                .Should().ThrowAsync<InvalidOperationException>()
                .WithMessage("Användaren hittades inte.");

            TestDbContextFactory.Destroy(context);
        }

        [Fact]
        public async Task Handle_Should_Throw_When_Shift_Overlaps()
        {
            // ARRANGE — Skapa en användare som redan har ett pass, försök skapa ett som krockar
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
            await context.SaveChangesAsync(CancellationToken.None);

            var validatorMock = new Mock<IValidator<CreateShiftCommand>>();
            validatorMock.Setup(v => v.ValidateAsync(It.IsAny<CreateShiftCommand>(), It.IsAny<CancellationToken>()))
                         .ReturnsAsync(new ValidationResult());

            var mockEmailService = new Mock<IEmailService>();
            var mockLogger = new Mock<ILogger<CreateShiftHandler>>();
            var handler = new CreateShiftHandler(context, validatorMock.Object, mockEmailService.Object, mockLogger.Object);

            // Nytt pass: 10:00 - 14:00 (krockar med 08-16!)
            var command = new CreateShiftCommand
            {
                UserId = userId,
                StartTime = DateTime.UtcNow.AddHours(10),
                EndTime = DateTime.UtcNow.AddHours(14)
            };

            // ACT & ASSERT — Ska kasta undantag vid passkrock
            await FluentActions.Invoking(() => handler.Handle(command, CancellationToken.None))
                .Should().ThrowAsync<InvalidOperationException>()
                .WithMessage("Denna användare har redan ett pass som krockar med den valda tiden.");

            TestDbContextFactory.Destroy(context);
        }
    }
}