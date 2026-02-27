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
        private static readonly Guid OrgId = Guid.NewGuid();

        // =====================================================================
        // UpdateShiftCommand
        // =====================================================================

        [Fact]
        public async Task UpdateShift_Should_Throw_When_Shift_Not_Found()
        {
            var context = TestDbContextFactory.Create();
            SeedOrg(context);

            var validatorMock = new Mock<IValidator<UpdateShiftCommand>>();
            validatorMock.Setup(v => v.ValidateAsync(It.IsAny<UpdateShiftCommand>(), It.IsAny<CancellationToken>()))
                         .ReturnsAsync(new ValidationResult());

            var handler = new UpdateShiftHandler(context, validatorMock.Object);

            var command = new UpdateShiftCommand
            {
                ShiftId = Guid.NewGuid(),
                StartTime = DateTime.UtcNow.AddHours(8),
                EndTime = DateTime.UtcNow.AddHours(16),
                OrganizationId = OrgId
            };

            await FluentActions.Invoking(() => handler.Handle(command, CancellationToken.None))
                .Should().ThrowAsync<InvalidOperationException>()
                .WithMessage("Passet hittades inte.");

            TestDbContextFactory.Destroy(context);
        }

        [Fact]
        public async Task UpdateShift_Should_Update_Times_Successfully()
        {
            var context = TestDbContextFactory.Create();
            SeedOrg(context);

            var shiftId = Guid.NewGuid();
            context.Shifts.Add(new Shift
            {
                Id = shiftId,
                StartTime = DateTime.UtcNow.AddHours(8),
                EndTime = DateTime.UtcNow.AddHours(16),
                UserId = null,
                OrganizationId = OrgId
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
                UserId = null,
                OrganizationId = OrgId
            };

            var result = await handler.Handle(command, CancellationToken.None);

            result.Should().BeTrue();
            var updatedShift = context.Shifts.First(s => s.Id == shiftId);
            updatedShift.StartTime.Hour.Should().Be(newStart.Hour);
            updatedShift.EndTime.Hour.Should().Be(newEnd.Hour);

            TestDbContextFactory.Destroy(context);
        }

        [Fact]
        public async Task UpdateShift_Should_Throw_When_Overlap_Detected()
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
                StartTime = DateTime.UtcNow.AddHours(8), EndTime = DateTime.UtcNow.AddHours(16)
            });

            var editShiftId = Guid.NewGuid();
            context.Shifts.Add(new Shift
            {
                Id = editShiftId, UserId = userId, OrganizationId = OrgId,
                StartTime = DateTime.UtcNow.AddHours(20), EndTime = DateTime.UtcNow.AddHours(22)
            });
            await context.SaveChangesAsync(CancellationToken.None);

            var validatorMock = new Mock<IValidator<UpdateShiftCommand>>();
            validatorMock.Setup(v => v.ValidateAsync(It.IsAny<UpdateShiftCommand>(), It.IsAny<CancellationToken>()))
                         .ReturnsAsync(new ValidationResult());

            var handler = new UpdateShiftHandler(context, validatorMock.Object);

            var command = new UpdateShiftCommand
            {
                ShiftId = editShiftId,
                StartTime = DateTime.UtcNow.AddHours(10),
                EndTime = DateTime.UtcNow.AddHours(14),
                UserId = userId,
                OrganizationId = OrgId
            };

            await FluentActions.Invoking(() => handler.Handle(command, CancellationToken.None))
                .Should().ThrowAsync<InvalidOperationException>()
                .WithMessage("Denna användare har redan ett pass som krockar med den valda tiden.");

            TestDbContextFactory.Destroy(context);
        }

        [Fact]
        public async Task UpdateShift_Should_Not_Overlap_With_Self()
        {
            var context = TestDbContextFactory.Create();
            SeedOrg(context);

            var userId = Guid.NewGuid();
            context.Users.Add(new User
            {
                Id = userId, FirstName = "Test", LastName = "Testsson",
                Email = "test@test.com", PasswordHash = "hash", Role = Role.Employee, OrganizationId = OrgId
            });

            var shiftId = Guid.NewGuid();
            context.Shifts.Add(new Shift
            {
                Id = shiftId, UserId = userId, OrganizationId = OrgId,
                StartTime = DateTime.UtcNow.AddHours(8), EndTime = DateTime.UtcNow.AddHours(16)
            });
            await context.SaveChangesAsync(CancellationToken.None);

            var validatorMock = new Mock<IValidator<UpdateShiftCommand>>();
            validatorMock.Setup(v => v.ValidateAsync(It.IsAny<UpdateShiftCommand>(), It.IsAny<CancellationToken>()))
                         .ReturnsAsync(new ValidationResult());

            var handler = new UpdateShiftHandler(context, validatorMock.Object);

            var command = new UpdateShiftCommand
            {
                ShiftId = shiftId,
                StartTime = DateTime.UtcNow.AddHours(9),
                EndTime = DateTime.UtcNow.AddHours(17),
                UserId = userId,
                OrganizationId = OrgId
            };

            var result = await handler.Handle(command, CancellationToken.None);
            result.Should().BeTrue();

            TestDbContextFactory.Destroy(context);
        }

        // =====================================================================
        // DeleteShiftCommand
        // =====================================================================

        [Fact]
        public async Task DeleteShift_Should_Throw_When_Shift_Not_Found()
        {
            var context = TestDbContextFactory.Create();
            var handler = new DeleteShiftHandler(context);

            await FluentActions.Invoking(() => handler.Handle(new DeleteShiftCommand(Guid.NewGuid(), OrgId), CancellationToken.None))
                .Should().ThrowAsync<InvalidOperationException>()
                .WithMessage("Passet hittades inte.");

            TestDbContextFactory.Destroy(context);
        }

        [Fact]
        public async Task DeleteShift_Should_Remove_Shift_Successfully()
        {
            var context = TestDbContextFactory.Create();
            SeedOrg(context);

            var shiftId = Guid.NewGuid();
            context.Shifts.Add(new Shift
            {
                Id = shiftId, OrganizationId = OrgId,
                StartTime = DateTime.UtcNow.AddHours(8),
                EndTime = DateTime.UtcNow.AddHours(16),
                UserId = null
            });
            await context.SaveChangesAsync(CancellationToken.None);

            var handler = new DeleteShiftHandler(context);
            var result = await handler.Handle(new DeleteShiftCommand(shiftId, OrgId), CancellationToken.None);

            result.Should().BeTrue();
            context.Shifts.Should().HaveCount(0);

            TestDbContextFactory.Destroy(context);
        }

        [Fact]
        public async Task DeleteShift_Should_Cancel_Pending_SwapRequests()
        {
            var context = TestDbContextFactory.Create();
            SeedOrg(context);

            var userId = Guid.NewGuid();
            context.Users.Add(new User
            {
                Id = userId, FirstName = "Test", LastName = "Testsson",
                Email = "test@test.com", PasswordHash = "hash", Role = Role.Employee, OrganizationId = OrgId
            });

            var requestingUserId = Guid.NewGuid();
            context.Users.Add(new User
            {
                Id = requestingUserId, FirstName = "Req", LastName = "Reqsson",
                Email = "req@test.com", PasswordHash = "hash", Role = Role.Employee, OrganizationId = OrgId
            });

            var shiftId = Guid.NewGuid();
            context.Shifts.Add(new Shift
            {
                Id = shiftId, OrganizationId = OrgId,
                StartTime = DateTime.UtcNow.AddHours(8),
                EndTime = DateTime.UtcNow.AddHours(16),
                UserId = userId,
                IsUpForSwap = true
            });

            context.SwapRequests.Add(new SwapRequest
            {
                Id = Guid.NewGuid(), ShiftId = shiftId,
                RequestingUserId = requestingUserId,
                Status = SwapRequestStatus.Pending,
                CreatedAt = DateTime.UtcNow
            });
            await context.SaveChangesAsync(CancellationToken.None);

            var handler = new DeleteShiftHandler(context);
            var result = await handler.Handle(new DeleteShiftCommand(shiftId, OrgId), CancellationToken.None);

            result.Should().BeTrue();
            context.Shifts.Should().HaveCount(0);
            context.SwapRequests.Should().HaveCount(0);

            TestDbContextFactory.Destroy(context);
        }

        private static void SeedOrg(Infrastructure.AppDbContext context)
        {
            context.Organizations.Add(new Organization { Id = OrgId, Name = "Test Org" });
            context.SaveChanges();
        }
    }
}
