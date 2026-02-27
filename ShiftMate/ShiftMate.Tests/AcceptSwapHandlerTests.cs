using FluentAssertions;
using ShiftMate.Application.SwapRequests.Commands;
using ShiftMate.Domain;
using ShiftMate.Tests.Support;
using Xunit;
using Moq;
using ShiftMate.Application.Interfaces;
using Microsoft.Extensions.Logging;

namespace ShiftMate.Tests
{
    public class AcceptSwapHandlerTests
    {
        private static readonly Guid OrgId = Guid.NewGuid();

        [Fact]
        public async Task Should_Throw_Exception_If_Shift_Overlaps()
        {
            var context = TestDbContextFactory.Create();
            SeedOrg(context);
            var mockEmailService = new Mock<IEmailService>();
            var mockLogger = new Mock<ILogger<AcceptSwapHandler>>();
            var handler = new AcceptSwapHandler(context, mockEmailService.Object, mockLogger.Object);

            var myUserId = Guid.NewGuid();
            var otherUserId = Guid.NewGuid();

            context.Users.Add(new User
            {
                Id = myUserId, FirstName = "André", LastName = "Testsson",
                Email = "andre@test.com", PasswordHash = "hash", Role = Role.Employee, OrganizationId = OrgId
            });
            context.Users.Add(new User
            {
                Id = otherUserId, FirstName = "Anna", LastName = "Testsson",
                Email = "anna@test.com", PasswordHash = "hash", Role = Role.Employee, OrganizationId = OrgId
            });

            var existingShift = new Shift
            {
                Id = Guid.NewGuid(), UserId = myUserId, OrganizationId = OrgId,
                StartTime = DateTime.UtcNow.AddHours(12), EndTime = DateTime.UtcNow.AddHours(16)
            };
            context.Shifts.Add(existingShift);

            var swapShift = new Shift
            {
                Id = Guid.NewGuid(), UserId = otherUserId, OrganizationId = OrgId,
                StartTime = DateTime.UtcNow.AddHours(10), EndTime = DateTime.UtcNow.AddHours(14),
                IsUpForSwap = true
            };
            context.Shifts.Add(swapShift);

            var swapRequest = new SwapRequest
            {
                Id = Guid.NewGuid(), ShiftId = swapShift.Id, RequestingUserId = otherUserId,
                Status = SwapRequestStatus.Pending
            };
            context.SwapRequests.Add(swapRequest);

            await context.SaveChangesAsync(CancellationToken.None);

            var command = new AcceptSwapCommand { SwapRequestId = swapRequest.Id, CurrentUserId = myUserId };

            await FluentActions.Invoking(() => handler.Handle(command, CancellationToken.None))
                .Should().ThrowAsync<Exception>()
                .WithMessage("Du har redan ett pass som krockar med detta!");

            TestDbContextFactory.Destroy(context);
        }

        [Fact]
        public async Task Should_Accept_Open_Swap_When_No_Overlap()
        {
            var context = TestDbContextFactory.Create();
            SeedOrg(context);
            var mockEmailService = new Mock<IEmailService>();
            var mockLogger = new Mock<ILogger<AcceptSwapHandler>>();
            var handler = new AcceptSwapHandler(context, mockEmailService.Object, mockLogger.Object);

            var acceptorId = Guid.NewGuid();
            var requesterId = Guid.NewGuid();

            context.Users.Add(new User
            {
                Id = acceptorId, FirstName = "Acceptor", LastName = "Testsson",
                Email = "acceptor@test.com", PasswordHash = "hash", Role = Role.Employee, OrganizationId = OrgId
            });
            context.Users.Add(new User
            {
                Id = requesterId, FirstName = "Requester", LastName = "Testsson",
                Email = "requester@test.com", PasswordHash = "hash", Role = Role.Employee, OrganizationId = OrgId
            });

            var swapShift = new Shift
            {
                Id = Guid.NewGuid(), UserId = requesterId, OrganizationId = OrgId,
                StartTime = DateTime.UtcNow.AddHours(10), EndTime = DateTime.UtcNow.AddHours(14),
                IsUpForSwap = true
            };
            context.Shifts.Add(swapShift);

            var swapRequest = new SwapRequest
            {
                Id = Guid.NewGuid(), ShiftId = swapShift.Id, RequestingUserId = requesterId,
                Status = SwapRequestStatus.Pending
            };
            context.SwapRequests.Add(swapRequest);

            await context.SaveChangesAsync(CancellationToken.None);

            var command = new AcceptSwapCommand { SwapRequestId = swapRequest.Id, CurrentUserId = acceptorId };
            await handler.Handle(command, CancellationToken.None);

            var updatedShift = context.Shifts.First(s => s.Id == swapShift.Id);
            updatedShift.UserId.Should().Be(acceptorId);
            updatedShift.IsUpForSwap.Should().BeFalse();

            var updatedRequest = context.SwapRequests.First(sr => sr.Id == swapRequest.Id);
            updatedRequest.Status.Should().Be(SwapRequestStatus.Accepted);

            TestDbContextFactory.Destroy(context);
        }

        [Fact]
        public async Task Should_Throw_When_Swap_Request_Not_Found()
        {
            var context = TestDbContextFactory.Create();
            var mockEmailService = new Mock<IEmailService>();
            var mockLogger = new Mock<ILogger<AcceptSwapHandler>>();
            var handler = new AcceptSwapHandler(context, mockEmailService.Object, mockLogger.Object);

            var command = new AcceptSwapCommand
            {
                SwapRequestId = Guid.NewGuid(),
                CurrentUserId = Guid.NewGuid()
            };

            await FluentActions.Invoking(() => handler.Handle(command, CancellationToken.None))
                .Should().ThrowAsync<Exception>()
                .WithMessage("Bytet hittades inte.");

            TestDbContextFactory.Destroy(context);
        }

        [Fact]
        public async Task Should_Throw_When_Swap_Already_Accepted()
        {
            var context = TestDbContextFactory.Create();
            SeedOrg(context);
            var mockEmailService = new Mock<IEmailService>();
            var mockLogger = new Mock<ILogger<AcceptSwapHandler>>();
            var handler = new AcceptSwapHandler(context, mockEmailService.Object, mockLogger.Object);

            var requesterId = Guid.NewGuid();

            context.Users.Add(new User
            {
                Id = requesterId, FirstName = "Requester", LastName = "Testsson",
                Email = "requester@test.com", PasswordHash = "hash", Role = Role.Employee, OrganizationId = OrgId
            });

            var swapShift = new Shift
            {
                Id = Guid.NewGuid(), UserId = requesterId, OrganizationId = OrgId,
                StartTime = DateTime.UtcNow.AddHours(10), EndTime = DateTime.UtcNow.AddHours(14),
                IsUpForSwap = true
            };
            context.Shifts.Add(swapShift);

            var swapRequest = new SwapRequest
            {
                Id = Guid.NewGuid(), ShiftId = swapShift.Id, RequestingUserId = requesterId,
                Status = SwapRequestStatus.Accepted
            };
            context.SwapRequests.Add(swapRequest);

            await context.SaveChangesAsync(CancellationToken.None);

            var command = new AcceptSwapCommand { SwapRequestId = swapRequest.Id, CurrentUserId = Guid.NewGuid() };

            await FluentActions.Invoking(() => handler.Handle(command, CancellationToken.None))
                .Should().ThrowAsync<Exception>()
                .WithMessage("Det här bytet är inte längre tillgängligt.");

            TestDbContextFactory.Destroy(context);
        }

        [Fact]
        public async Task Should_Accept_Direct_Swap_On_Same_Day()
        {
            var context = TestDbContextFactory.Create();
            SeedOrg(context);
            var mockEmailService = new Mock<IEmailService>();
            var mockLogger = new Mock<ILogger<AcceptSwapHandler>>();
            var handler = new AcceptSwapHandler(context, mockEmailService.Object, mockLogger.Object);

            var userAId = Guid.NewGuid();
            var userBId = Guid.NewGuid();

            context.Users.Add(new User
            {
                Id = userAId, FirstName = "UserA", LastName = "Testsson",
                Email = "usera@test.com", PasswordHash = "hash", Role = Role.Employee, OrganizationId = OrgId
            });
            context.Users.Add(new User
            {
                Id = userBId, FirstName = "UserB", LastName = "Testsson",
                Email = "userb@test.com", PasswordHash = "hash", Role = Role.Employee, OrganizationId = OrgId
            });

            var shiftA = new Shift
            {
                Id = Guid.NewGuid(), UserId = userAId, OrganizationId = OrgId,
                StartTime = DateTime.UtcNow.Date.AddDays(1).AddHours(8),
                EndTime = DateTime.UtcNow.Date.AddDays(1).AddHours(12),
                IsUpForSwap = true
            };

            var shiftB = new Shift
            {
                Id = Guid.NewGuid(), UserId = userBId, OrganizationId = OrgId,
                StartTime = DateTime.UtcNow.Date.AddDays(1).AddHours(13),
                EndTime = DateTime.UtcNow.Date.AddDays(1).AddHours(17),
                IsUpForSwap = false
            };

            context.Shifts.Add(shiftA);
            context.Shifts.Add(shiftB);

            var swapRequest = new SwapRequest
            {
                Id = Guid.NewGuid(), ShiftId = shiftA.Id, RequestingUserId = userAId,
                TargetUserId = userBId, TargetShiftId = shiftB.Id,
                Status = SwapRequestStatus.Pending
            };
            context.SwapRequests.Add(swapRequest);

            await context.SaveChangesAsync(CancellationToken.None);

            var command = new AcceptSwapCommand { SwapRequestId = swapRequest.Id, CurrentUserId = userBId };
            await handler.Handle(command, CancellationToken.None);

            var updatedShiftA = context.Shifts.First(s => s.Id == shiftA.Id);
            var updatedShiftB = context.Shifts.First(s => s.Id == shiftB.Id);

            updatedShiftA.UserId.Should().Be(userBId, "User B ska nu äga User A:s gamla pass");
            updatedShiftB.UserId.Should().Be(userAId, "User A ska nu äga User B:s gamla pass");
            updatedShiftA.IsUpForSwap.Should().BeFalse();
            updatedShiftB.IsUpForSwap.Should().BeFalse();

            var updatedRequest = context.SwapRequests.First(sr => sr.Id == swapRequest.Id);
            updatedRequest.Status.Should().Be(SwapRequestStatus.Accepted);

            TestDbContextFactory.Destroy(context);
        }

        [Fact]
        public async Task Should_Accept_Direct_Swap_With_Overlapping_Times()
        {
            var context = TestDbContextFactory.Create();
            SeedOrg(context);
            var mockEmailService = new Mock<IEmailService>();
            var mockLogger = new Mock<ILogger<AcceptSwapHandler>>();
            var handler = new AcceptSwapHandler(context, mockEmailService.Object, mockLogger.Object);

            var userAId = Guid.NewGuid();
            var userBId = Guid.NewGuid();

            context.Users.Add(new User
            {
                Id = userAId, FirstName = "UserA", LastName = "Testsson",
                Email = "usera@test.com", PasswordHash = "hash", Role = Role.Employee, OrganizationId = OrgId
            });
            context.Users.Add(new User
            {
                Id = userBId, FirstName = "UserB", LastName = "Testsson",
                Email = "userb@test.com", PasswordHash = "hash", Role = Role.Employee, OrganizationId = OrgId
            });

            var shiftA = new Shift
            {
                Id = Guid.NewGuid(), UserId = userAId, OrganizationId = OrgId,
                StartTime = DateTime.UtcNow.Date.AddDays(1).AddHours(8),
                EndTime = DateTime.UtcNow.Date.AddDays(1).AddHours(14),
                IsUpForSwap = true
            };

            var shiftB = new Shift
            {
                Id = Guid.NewGuid(), UserId = userBId, OrganizationId = OrgId,
                StartTime = DateTime.UtcNow.Date.AddDays(1).AddHours(10),
                EndTime = DateTime.UtcNow.Date.AddDays(1).AddHours(16),
                IsUpForSwap = false
            };

            context.Shifts.Add(shiftA);
            context.Shifts.Add(shiftB);

            var swapRequest = new SwapRequest
            {
                Id = Guid.NewGuid(), ShiftId = shiftA.Id, RequestingUserId = userAId,
                TargetUserId = userBId, TargetShiftId = shiftB.Id,
                Status = SwapRequestStatus.Pending
            };
            context.SwapRequests.Add(swapRequest);

            await context.SaveChangesAsync(CancellationToken.None);

            var command = new AcceptSwapCommand { SwapRequestId = swapRequest.Id, CurrentUserId = userBId };
            await handler.Handle(command, CancellationToken.None);

            var updatedShiftA = context.Shifts.First(s => s.Id == shiftA.Id);
            var updatedShiftB = context.Shifts.First(s => s.Id == shiftB.Id);

            updatedShiftA.UserId.Should().Be(userBId);
            updatedShiftB.UserId.Should().Be(userAId);

            TestDbContextFactory.Destroy(context);
        }

        [Fact]
        public async Task Should_Reject_Direct_Swap_When_Third_Shift_Causes_Overlap()
        {
            var context = TestDbContextFactory.Create();
            SeedOrg(context);
            var mockEmailService = new Mock<IEmailService>();
            var mockLogger = new Mock<ILogger<AcceptSwapHandler>>();
            var handler = new AcceptSwapHandler(context, mockEmailService.Object, mockLogger.Object);

            var userAId = Guid.NewGuid();
            var userBId = Guid.NewGuid();

            context.Users.Add(new User
            {
                Id = userAId, FirstName = "UserA", LastName = "Testsson",
                Email = "usera@test.com", PasswordHash = "hash", Role = Role.Employee, OrganizationId = OrgId
            });
            context.Users.Add(new User
            {
                Id = userBId, FirstName = "UserB", LastName = "Testsson",
                Email = "userb@test.com", PasswordHash = "hash", Role = Role.Employee, OrganizationId = OrgId
            });

            var shiftA = new Shift
            {
                Id = Guid.NewGuid(), UserId = userAId, OrganizationId = OrgId,
                StartTime = DateTime.UtcNow.Date.AddDays(1).AddHours(8),
                EndTime = DateTime.UtcNow.Date.AddDays(1).AddHours(12),
                IsUpForSwap = true
            };

            var shiftAExtra = new Shift
            {
                Id = Guid.NewGuid(), UserId = userAId, OrganizationId = OrgId,
                StartTime = DateTime.UtcNow.Date.AddDays(1).AddHours(14),
                EndTime = DateTime.UtcNow.Date.AddDays(1).AddHours(18)
            };

            var shiftB = new Shift
            {
                Id = Guid.NewGuid(), UserId = userBId, OrganizationId = OrgId,
                StartTime = DateTime.UtcNow.Date.AddDays(1).AddHours(15),
                EndTime = DateTime.UtcNow.Date.AddDays(1).AddHours(19),
                IsUpForSwap = false
            };

            context.Shifts.AddRange(shiftA, shiftAExtra, shiftB);

            var swapRequest = new SwapRequest
            {
                Id = Guid.NewGuid(), ShiftId = shiftA.Id, RequestingUserId = userAId,
                TargetUserId = userBId, TargetShiftId = shiftB.Id,
                Status = SwapRequestStatus.Pending
            };
            context.SwapRequests.Add(swapRequest);

            await context.SaveChangesAsync(CancellationToken.None);

            var command = new AcceptSwapCommand { SwapRequestId = swapRequest.Id, CurrentUserId = userBId };

            await FluentActions.Invoking(() => handler.Handle(command, CancellationToken.None))
                .Should().ThrowAsync<Exception>()
                .WithMessage("*passkrock*");

            TestDbContextFactory.Destroy(context);
        }

        private static void SeedOrg(Infrastructure.AppDbContext context)
        {
            context.Organizations.Add(new Organization { Id = OrgId, Name = "Test Org" });
            context.SaveChanges();
        }
    }
}
