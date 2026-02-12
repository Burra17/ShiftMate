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
        [Fact]
        public async Task Should_Throw_Exception_If_Shift_Overlaps()
        {
            // 1. ARRANGE
            var context = TestDbContextFactory.Create();
            var mockEmailService = new Mock<IEmailService>();
            var mockLogger = new Mock<ILogger<AcceptSwapHandler>>();
            var handler = new AcceptSwapHandler(context, mockEmailService.Object, mockLogger.Object);

            var myUserId = Guid.NewGuid();
            var otherUserId = Guid.NewGuid();

            // Skapa användare i databasen (krävs för Include() och FK-relationer)
            context.Users.Add(new User
            {
                Id = myUserId,
                FirstName = "André",
                LastName = "Testsson",
                Email = "andre@test.com",
                PasswordHash = "hash",
                Role = Role.Employee
            });
            context.Users.Add(new User
            {
                Id = otherUserId,
                FirstName = "Anna",
                LastName = "Testsson",
                Email = "anna@test.com",
                PasswordHash = "hash",
                Role = Role.Employee
            });

            // A. André jobbar redan 12:00 - 16:00
            var existingShift = new Shift
            {
                Id = Guid.NewGuid(),
                UserId = myUserId,
                StartTime = DateTime.UtcNow.AddHours(12),
                EndTime = DateTime.UtcNow.AddHours(16)
            };
            context.Shifts.Add(existingShift);

            // B. Det finns ett pass ute för byte: 10:00 - 14:00 (KROCKAR med Andrés!)
            var swapShift = new Shift
            {
                Id = Guid.NewGuid(),
                UserId = otherUserId,
                StartTime = DateTime.UtcNow.AddHours(10),
                EndTime = DateTime.UtcNow.AddHours(14),
                IsUpForSwap = true
            };
            context.Shifts.Add(swapShift);

            // C. Skapa bytesförfrågan för det passet
            var swapRequest = new SwapRequest
            {
                Id = Guid.NewGuid(),
                ShiftId = swapShift.Id,
                RequestingUserId = otherUserId,
                Status = "Pending"
            };
            context.SwapRequests.Add(swapRequest);

            await context.SaveChangesAsync(CancellationToken.None);

            // 2. ACT & ASSERT — Ska kasta fel vid passkrock
            var command = new AcceptSwapCommand { SwapRequestId = swapRequest.Id, CurrentUserId = myUserId };

            await FluentActions.Invoking(() => handler.Handle(command, CancellationToken.None))
                .Should().ThrowAsync<Exception>()
                .WithMessage("Du har redan ett pass som krockar med detta!");

            TestDbContextFactory.Destroy(context);
        }

        [Fact]
        public async Task Should_Accept_Open_Swap_When_No_Overlap()
        {
            // ARRANGE — Skapa ett öppet byte utan krock
            var context = TestDbContextFactory.Create();
            var mockEmailService = new Mock<IEmailService>();
            var mockLogger = new Mock<ILogger<AcceptSwapHandler>>();
            var handler = new AcceptSwapHandler(context, mockEmailService.Object, mockLogger.Object);

            var acceptorId = Guid.NewGuid();
            var requesterId = Guid.NewGuid();

            context.Users.Add(new User
            {
                Id = acceptorId,
                FirstName = "Acceptor",
                LastName = "Testsson",
                Email = "acceptor@test.com",
                PasswordHash = "hash",
                Role = Role.Employee
            });
            context.Users.Add(new User
            {
                Id = requesterId,
                FirstName = "Requester",
                LastName = "Testsson",
                Email = "requester@test.com",
                PasswordHash = "hash",
                Role = Role.Employee
            });

            // Passet som erbjuds: 10:00 - 14:00
            var swapShift = new Shift
            {
                Id = Guid.NewGuid(),
                UserId = requesterId,
                StartTime = DateTime.UtcNow.AddHours(10),
                EndTime = DateTime.UtcNow.AddHours(14),
                IsUpForSwap = true
            };
            context.Shifts.Add(swapShift);

            var swapRequest = new SwapRequest
            {
                Id = Guid.NewGuid(),
                ShiftId = swapShift.Id,
                RequestingUserId = requesterId,
                Status = "Pending"
            };
            context.SwapRequests.Add(swapRequest);

            await context.SaveChangesAsync(CancellationToken.None);

            // ACT — Acceptera bytet (acceptorn har inga krockande pass)
            var command = new AcceptSwapCommand { SwapRequestId = swapRequest.Id, CurrentUserId = acceptorId };
            await handler.Handle(command, CancellationToken.None);

            // ASSERT — Passet ska nu tillhöra acceptorn
            var updatedShift = context.Shifts.First(s => s.Id == swapShift.Id);
            updatedShift.UserId.Should().Be(acceptorId);
            updatedShift.IsUpForSwap.Should().BeFalse();

            var updatedRequest = context.SwapRequests.First(sr => sr.Id == swapRequest.Id);
            updatedRequest.Status.Should().Be("Accepted");

            TestDbContextFactory.Destroy(context);
        }

        [Fact]
        public async Task Should_Throw_When_Swap_Request_Not_Found()
        {
            // ARRANGE — Försök acceptera ett byte som inte finns
            var context = TestDbContextFactory.Create();
            var mockEmailService = new Mock<IEmailService>();
            var mockLogger = new Mock<ILogger<AcceptSwapHandler>>();
            var handler = new AcceptSwapHandler(context, mockEmailService.Object, mockLogger.Object);

            var command = new AcceptSwapCommand
            {
                SwapRequestId = Guid.NewGuid(),
                CurrentUserId = Guid.NewGuid()
            };

            // ACT & ASSERT
            await FluentActions.Invoking(() => handler.Handle(command, CancellationToken.None))
                .Should().ThrowAsync<Exception>()
                .WithMessage("Bytet hittades inte.");

            TestDbContextFactory.Destroy(context);
        }

        [Fact]
        public async Task Should_Throw_When_Swap_Already_Accepted()
        {
            // ARRANGE — Skapa ett byte som redan accepterats
            var context = TestDbContextFactory.Create();
            var mockEmailService = new Mock<IEmailService>();
            var mockLogger = new Mock<ILogger<AcceptSwapHandler>>();
            var handler = new AcceptSwapHandler(context, mockEmailService.Object, mockLogger.Object);

            var requesterId = Guid.NewGuid();

            context.Users.Add(new User
            {
                Id = requesterId,
                FirstName = "Requester",
                LastName = "Testsson",
                Email = "requester@test.com",
                PasswordHash = "hash",
                Role = Role.Employee
            });

            var swapShift = new Shift
            {
                Id = Guid.NewGuid(),
                UserId = requesterId,
                StartTime = DateTime.UtcNow.AddHours(10),
                EndTime = DateTime.UtcNow.AddHours(14),
                IsUpForSwap = true
            };
            context.Shifts.Add(swapShift);

            var swapRequest = new SwapRequest
            {
                Id = Guid.NewGuid(),
                ShiftId = swapShift.Id,
                RequestingUserId = requesterId,
                Status = "Accepted" // Redan accepterat!
            };
            context.SwapRequests.Add(swapRequest);

            await context.SaveChangesAsync(CancellationToken.None);

            // ACT & ASSERT — Ska kasta fel för redan accepterat byte
            var command = new AcceptSwapCommand { SwapRequestId = swapRequest.Id, CurrentUserId = Guid.NewGuid() };

            await FluentActions.Invoking(() => handler.Handle(command, CancellationToken.None))
                .Should().ThrowAsync<Exception>()
                .WithMessage("Det här bytet är inte längre tillgängligt.");

            TestDbContextFactory.Destroy(context);
        }

        [Fact]
        public async Task Should_Accept_Direct_Swap_On_Same_Day()
        {
            // ARRANGE — Två kollegor byter pass på samma dag (onsdag-scenariot)
            var context = TestDbContextFactory.Create();
            var mockEmailService = new Mock<IEmailService>();
            var mockLogger = new Mock<ILogger<AcceptSwapHandler>>();
            var handler = new AcceptSwapHandler(context, mockEmailService.Object, mockLogger.Object);

            var userAId = Guid.NewGuid();
            var userBId = Guid.NewGuid();

            context.Users.Add(new User
            {
                Id = userAId,
                FirstName = "UserA",
                LastName = "Testsson",
                Email = "usera@test.com",
                PasswordHash = "hash",
                Role = Role.Employee
            });
            context.Users.Add(new User
            {
                Id = userBId,
                FirstName = "UserB",
                LastName = "Testsson",
                Email = "userb@test.com",
                PasswordHash = "hash",
                Role = Role.Employee
            });

            // User A:s pass: onsdag 08:00 - 12:00
            var shiftA = new Shift
            {
                Id = Guid.NewGuid(),
                UserId = userAId,
                StartTime = DateTime.UtcNow.Date.AddDays(1).AddHours(8),
                EndTime = DateTime.UtcNow.Date.AddDays(1).AddHours(12),
                IsUpForSwap = true
            };

            // User B:s pass: onsdag 13:00 - 17:00
            var shiftB = new Shift
            {
                Id = Guid.NewGuid(),
                UserId = userBId,
                StartTime = DateTime.UtcNow.Date.AddDays(1).AddHours(13),
                EndTime = DateTime.UtcNow.Date.AddDays(1).AddHours(17),
                IsUpForSwap = false
            };

            context.Shifts.Add(shiftA);
            context.Shifts.Add(shiftB);

            // User A föreslår: "Jag ger mitt pass (shiftA) i utbyte mot ditt (shiftB)"
            var swapRequest = new SwapRequest
            {
                Id = Guid.NewGuid(),
                ShiftId = shiftA.Id,
                RequestingUserId = userAId,
                TargetUserId = userBId,
                TargetShiftId = shiftB.Id,
                Status = "Pending"
            };
            context.SwapRequests.Add(swapRequest);

            await context.SaveChangesAsync(CancellationToken.None);

            // ACT — User B accepterar bytet
            var command = new AcceptSwapCommand { SwapRequestId = swapRequest.Id, CurrentUserId = userBId };
            await handler.Handle(command, CancellationToken.None);

            // ASSERT — Passen ska ha bytt ägare
            var updatedShiftA = context.Shifts.First(s => s.Id == shiftA.Id);
            var updatedShiftB = context.Shifts.First(s => s.Id == shiftB.Id);

            updatedShiftA.UserId.Should().Be(userBId, "User B ska nu äga User A:s gamla pass");
            updatedShiftB.UserId.Should().Be(userAId, "User A ska nu äga User B:s gamla pass");
            updatedShiftA.IsUpForSwap.Should().BeFalse();
            updatedShiftB.IsUpForSwap.Should().BeFalse();

            var updatedRequest = context.SwapRequests.First(sr => sr.Id == swapRequest.Id);
            updatedRequest.Status.Should().Be("Accepted");

            TestDbContextFactory.Destroy(context);
        }

        [Fact]
        public async Task Should_Accept_Direct_Swap_With_Overlapping_Times()
        {
            // ARRANGE — Två kollegor byter ÖVERLAPPANDE pass på samma dag
            // Detta är exakt buggen: pass som överlappar ska kunna bytas
            var context = TestDbContextFactory.Create();
            var mockEmailService = new Mock<IEmailService>();
            var mockLogger = new Mock<ILogger<AcceptSwapHandler>>();
            var handler = new AcceptSwapHandler(context, mockEmailService.Object, mockLogger.Object);

            var userAId = Guid.NewGuid();
            var userBId = Guid.NewGuid();

            context.Users.Add(new User
            {
                Id = userAId,
                FirstName = "UserA",
                LastName = "Testsson",
                Email = "usera@test.com",
                PasswordHash = "hash",
                Role = Role.Employee
            });
            context.Users.Add(new User
            {
                Id = userBId,
                FirstName = "UserB",
                LastName = "Testsson",
                Email = "userb@test.com",
                PasswordHash = "hash",
                Role = Role.Employee
            });

            // User A:s pass: onsdag 08:00 - 14:00
            var shiftA = new Shift
            {
                Id = Guid.NewGuid(),
                UserId = userAId,
                StartTime = DateTime.UtcNow.Date.AddDays(1).AddHours(8),
                EndTime = DateTime.UtcNow.Date.AddDays(1).AddHours(14),
                IsUpForSwap = true
            };

            // User B:s pass: onsdag 10:00 - 16:00 (ÖVERLAPPAR med A:s pass!)
            var shiftB = new Shift
            {
                Id = Guid.NewGuid(),
                UserId = userBId,
                StartTime = DateTime.UtcNow.Date.AddDays(1).AddHours(10),
                EndTime = DateTime.UtcNow.Date.AddDays(1).AddHours(16),
                IsUpForSwap = false
            };

            context.Shifts.Add(shiftA);
            context.Shifts.Add(shiftB);

            // User A föreslår direktbyte
            var swapRequest = new SwapRequest
            {
                Id = Guid.NewGuid(),
                ShiftId = shiftA.Id,
                RequestingUserId = userAId,
                TargetUserId = userBId,
                TargetShiftId = shiftB.Id,
                Status = "Pending"
            };
            context.SwapRequests.Add(swapRequest);

            await context.SaveChangesAsync(CancellationToken.None);

            // ACT — User B accepterar (ska fungera trots överlapp — de byter ju!)
            var command = new AcceptSwapCommand { SwapRequestId = swapRequest.Id, CurrentUserId = userBId };
            await handler.Handle(command, CancellationToken.None);

            // ASSERT — Passen ska ha bytt ägare
            var updatedShiftA = context.Shifts.First(s => s.Id == shiftA.Id);
            var updatedShiftB = context.Shifts.First(s => s.Id == shiftB.Id);

            updatedShiftA.UserId.Should().Be(userBId);
            updatedShiftB.UserId.Should().Be(userAId);

            TestDbContextFactory.Destroy(context);
        }

        [Fact]
        public async Task Should_Reject_Direct_Swap_When_Third_Shift_Causes_Overlap()
        {
            // ARRANGE — User A har ETT EXTRA pass som krockar med passet hen får
            // Bytet ska avvisas eftersom User A redan har ett TREDJE pass som krockar
            var context = TestDbContextFactory.Create();
            var mockEmailService = new Mock<IEmailService>();
            var mockLogger = new Mock<ILogger<AcceptSwapHandler>>();
            var handler = new AcceptSwapHandler(context, mockEmailService.Object, mockLogger.Object);

            var userAId = Guid.NewGuid();
            var userBId = Guid.NewGuid();

            context.Users.Add(new User
            {
                Id = userAId,
                FirstName = "UserA",
                LastName = "Testsson",
                Email = "usera@test.com",
                PasswordHash = "hash",
                Role = Role.Employee
            });
            context.Users.Add(new User
            {
                Id = userBId,
                FirstName = "UserB",
                LastName = "Testsson",
                Email = "userb@test.com",
                PasswordHash = "hash",
                Role = Role.Employee
            });

            // User A:s pass som ska bytas bort: onsdag 08:00 - 12:00
            var shiftA = new Shift
            {
                Id = Guid.NewGuid(),
                UserId = userAId,
                StartTime = DateTime.UtcNow.Date.AddDays(1).AddHours(8),
                EndTime = DateTime.UtcNow.Date.AddDays(1).AddHours(12),
                IsUpForSwap = true
            };

            // User A:s ANDRA pass: onsdag 14:00 - 18:00 (krockar med B:s pass!)
            var shiftAExtra = new Shift
            {
                Id = Guid.NewGuid(),
                UserId = userAId,
                StartTime = DateTime.UtcNow.Date.AddDays(1).AddHours(14),
                EndTime = DateTime.UtcNow.Date.AddDays(1).AddHours(18)
            };

            // User B:s pass: onsdag 15:00 - 19:00 (krockar med A:s extra-pass!)
            var shiftB = new Shift
            {
                Id = Guid.NewGuid(),
                UserId = userBId,
                StartTime = DateTime.UtcNow.Date.AddDays(1).AddHours(15),
                EndTime = DateTime.UtcNow.Date.AddDays(1).AddHours(19),
                IsUpForSwap = false
            };

            context.Shifts.AddRange(shiftA, shiftAExtra, shiftB);

            var swapRequest = new SwapRequest
            {
                Id = Guid.NewGuid(),
                ShiftId = shiftA.Id,
                RequestingUserId = userAId,
                TargetUserId = userBId,
                TargetShiftId = shiftB.Id,
                Status = "Pending"
            };
            context.SwapRequests.Add(swapRequest);

            await context.SaveChangesAsync(CancellationToken.None);

            // ACT & ASSERT — Ska avvisas: User A:s extra-pass krockar med passet hen skulle få
            var command = new AcceptSwapCommand { SwapRequestId = swapRequest.Id, CurrentUserId = userBId };

            await FluentActions.Invoking(() => handler.Handle(command, CancellationToken.None))
                .Should().ThrowAsync<Exception>()
                .WithMessage("*passkrock*");

            TestDbContextFactory.Destroy(context);
        }
    }
}