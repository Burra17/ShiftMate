using FluentAssertions;
using FluentValidation;
using FluentValidation.Results;
using Moq; // <-- Vår stuntman-fabrik
using ShiftMate.Application.Shifts.Commands;
using ShiftMate.Tests.Support;
using Xunit;

namespace ShiftMate.Tests
{
    public class CreateShiftHandlerTests
    {
        [Fact]
        public async Task Handle_Should_Save_Shift_To_Database()
        {
            // 1. ARRANGE (Förbered)
            // Skapa en låtsas-databas
            var context = TestDbContextFactory.Create();

            // Skapa en "Stuntman" (Mock) för Validatorn
            // Vi säger åt den: "Oavsett vad du får in, svara alltid att det är Valid!"
            var validatorMock = new Mock<IValidator<CreateShiftCommand>>();
            validatorMock.Setup(v => v.ValidateAsync(It.IsAny<CreateShiftCommand>(), It.IsAny<CancellationToken>()))
                         .ReturnsAsync(new ValidationResult()); // Returnera tomt resultat = Inga fel

            // Skapa handlern med låtsas-databasen och stunt-validatorn
            var handler = new CreateShiftHandler(context, validatorMock.Object);

            var command = new CreateShiftCommand
            {
                UserId = Guid.NewGuid(),
                StartTime = DateTime.UtcNow.AddHours(8),
                EndTime = DateTime.UtcNow.AddHours(16)
            };

            // 2. ACT (Utför)
            var resultId = await handler.Handle(command, CancellationToken.None);

            // 3. ASSERT (Kontrollera)
            // Kolla att vi fick tillbaka ett ID
            resultId.Should().NotBeEmpty();

            // Kolla att det nu finns EXAKT 1 pass i databasen
            context.Shifts.Should().HaveCount(1);

            // Kolla att passet i databasen har rätt UserId
            context.Shifts.First().UserId.Should().Be(command.UserId);

            // Städa upp
            TestDbContextFactory.Destroy(context);
        }
    }
}