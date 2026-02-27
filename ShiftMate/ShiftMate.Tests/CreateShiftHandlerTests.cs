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
        private static readonly Guid OrgId = Guid.NewGuid();

        [Fact]
        public async Task Handle_Should_Save_Shift_To_Database()
        {
            var context = TestDbContextFactory.Create();
            SeedOrg(context);

            var userId = Guid.NewGuid();
            context.Users.Add(new User
            {
                Id = userId, FirstName = "Test", LastName = "Testsson",
                Email = "test@test.com", PasswordHash = "hash", Role = Role.Employee, OrganizationId = OrgId
            });
            await context.SaveChangesAsync(CancellationToken.None);

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
                EndTime = DateTime.UtcNow.AddHours(16),
                OrganizationId = OrgId
            };

            var resultId = await handler.Handle(command, CancellationToken.None);

            resultId.Should().NotBeEmpty();
            context.Shifts.Should().HaveCount(1);
            context.Shifts.First().UserId.Should().Be(userId);
            context.Shifts.First().OrganizationId.Should().Be(OrgId);

            TestDbContextFactory.Destroy(context);
        }

        [Fact]
        public async Task Handle_Should_Create_Open_Shift_Without_UserId()
        {
            var context = TestDbContextFactory.Create();
            SeedOrg(context);

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
                EndTime = DateTime.UtcNow.AddHours(16),
                OrganizationId = OrgId
            };

            var resultId = await handler.Handle(command, CancellationToken.None);

            resultId.Should().NotBeEmpty();
            context.Shifts.Should().HaveCount(1);
            context.Shifts.First().UserId.Should().BeNull();

            TestDbContextFactory.Destroy(context);
        }

        [Fact]
        public async Task Handle_Should_Throw_When_User_Not_Found()
        {
            var context = TestDbContextFactory.Create();
            SeedOrg(context);

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
                EndTime = DateTime.UtcNow.AddHours(16),
                OrganizationId = OrgId
            };

            await FluentActions.Invoking(() => handler.Handle(command, CancellationToken.None))
                .Should().ThrowAsync<InvalidOperationException>()
                .WithMessage("Användaren hittades inte.");

            TestDbContextFactory.Destroy(context);
        }

        [Fact]
        public async Task Handle_Should_Throw_When_Shift_Overlaps()
        {
            var context = TestDbContextFactory.Create();
            SeedOrg(context);

            var userId = Guid.NewGuid();
            context.Users.Add(new User
            {
                Id = userId, FirstName = "Test", LastName = "Testsson",
                Email = "test@test.com", PasswordHash = "hash", Role = Role.Employee, OrganizationId = OrgId
            });

            context.Shifts.Add(new Shift
            {
                Id = Guid.NewGuid(), UserId = userId, OrganizationId = OrgId,
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

            var command = new CreateShiftCommand
            {
                UserId = userId,
                StartTime = DateTime.UtcNow.AddHours(10),
                EndTime = DateTime.UtcNow.AddHours(14),
                OrganizationId = OrgId
            };

            await FluentActions.Invoking(() => handler.Handle(command, CancellationToken.None))
                .Should().ThrowAsync<InvalidOperationException>()
                .WithMessage("Denna användare har redan ett pass som krockar med den valda tiden.");

            TestDbContextFactory.Destroy(context);
        }

        private static void SeedOrg(Infrastructure.AppDbContext context)
        {
            context.Organizations.Add(new Organization { Id = OrgId, Name = "Test Org" });
            context.SaveChanges();
        }
    }
}
