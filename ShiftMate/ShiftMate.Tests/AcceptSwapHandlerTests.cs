using FluentAssertions;
using ShiftMate.Application.SwapRequests.Commands;
using ShiftMate.Domain; // Kan heta ShiftMate.Domain.Entities beroende på din struktur
using ShiftMate.Tests.Support;
using Xunit;

namespace ShiftMate.Tests
{
    public class AcceptSwapHandlerTests
    {
        [Fact]
        public async Task Should_Throw_Exception_If_Shift_Overlaps()
        {
            // 1. ARRANGE
            var context = TestDbContextFactory.Create();
            var handler = new AcceptSwapHandler(context);

            var myUserId = Guid.NewGuid();

            // A. André jobbar redan 12:00 - 16:00
            var existingShift = new Shift
            {
                Id = Guid.NewGuid(),
                UserId = myUserId,
                StartTime = DateTime.UtcNow.AddHours(12),
                EndTime = DateTime.UtcNow.AddHours(16)
            };
            context.Shifts.Add(existingShift);

            // B. Det finns ett pass ute för byte: 10:00 - 14:00 (KROCKAR!)
            var swapShift = new Shift
            {
                Id = Guid.NewGuid(),
                UserId = Guid.NewGuid(), // Någon annan äger detta
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
                RequestingUserId = swapShift.UserId.Value,
                Status = "Pending"
            };
            context.SwapRequests.Add(swapRequest);

            await context.SaveChangesAsync(CancellationToken.None);

            // 2. ACT & ASSERT
            // Vi försöker acceptera bytet. Detta SKA kasta ett fel om koden funkar som den ska.
            var command = new AcceptSwapCommand { SwapRequestId = swapRequest.Id, CurrentUserId = myUserId };

            await FluentActions.Invoking(() => handler.Handle(command, CancellationToken.None))
                .Should().ThrowAsync<Exception>()
                .WithMessage("Du har redan ett pass som krockar med detta!");

            // Städa
            TestDbContextFactory.Destroy(context);
        }
    }
}