using FluentValidation.TestHelper; // Hjälper oss testa validators
using ShiftMate.Application.Shifts.Commands;
using Xunit; // Själva testmotorn

namespace ShiftMate.Tests
{
    public class CreateShiftCommandValidatorTests
    {
        private readonly CreateShiftCommandValidator _validator;

        public CreateShiftCommandValidatorTests()
        {
            // Vi skapar en instans av din "Ordningsvakt" inför varje test
            _validator = new CreateShiftCommandValidator();
        }

        [Fact] // [Fact] betyder "Detta är ett test"
        public void Should_Have_Error_When_EndTime_Is_Before_StartTime()
        {
            // 1. Arrange (Förberedelse)
            var command = new CreateShiftCommand
            {
                StartTime = DateTime.UtcNow.AddHours(12),
                EndTime = DateTime.UtcNow.AddHours(10) // FEL: Slutar före start!
            };

            // 2. Act (Utför testet)
            var result = _validator.TestValidate(command);

            // 3. Assert (Kontrollera resultatet)
            // Vi förväntar oss ett fel på fältet "EndTime"
            result.ShouldHaveValidationErrorFor(x => x.EndTime)
                  .WithErrorMessage("Passet kan inte sluta innan det har börjat.");
        }

        [Fact]
        public void Should_Not_Have_Error_When_Time_Is_Correct()
        {
            // 1. Arrange
            var command = new CreateShiftCommand
            {
                StartTime = DateTime.UtcNow.AddHours(10),
                EndTime = DateTime.UtcNow.AddHours(12) // RÄTT: Slutar efter start
            };

            // 2. Act
            var result = _validator.TestValidate(command);

            // 3. Assert
            result.ShouldNotHaveValidationErrorFor(x => x.EndTime);
        }
    }
}