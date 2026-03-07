using FluentValidation.TestHelper;
using ShiftMate.Application.Shifts.Commands;

namespace ShiftMate.Tests
{
    public class UpdateShiftCommandValidatorTests
    {
        private readonly UpdateShiftCommandValidator _validator = new();

        [Fact]
        public void Should_Not_Have_Errors_When_Valid()
        {
            var command = new UpdateShiftCommand
            {
                ShiftId = Guid.NewGuid(),
                StartTime = DateTime.UtcNow.AddHours(1),
                EndTime = DateTime.UtcNow.AddHours(9),
                UserId = Guid.NewGuid()
            };

            var result = _validator.TestValidate(command);

            result.ShouldNotHaveAnyValidationErrors();
        }

        [Fact]
        public void Should_Have_Error_When_StartTime_Is_Empty()
        {
            var command = new UpdateShiftCommand
            {
                ShiftId = Guid.NewGuid(),
                StartTime = default,
                EndTime = DateTime.UtcNow.AddHours(9)
            };

            var result = _validator.TestValidate(command);

            result.ShouldHaveValidationErrorFor(x => x.StartTime)
                  .WithErrorMessage("Starttid måste anges.");
        }

        [Fact]
        public void Should_Have_Error_When_EndTime_Is_Empty()
        {
            var command = new UpdateShiftCommand
            {
                ShiftId = Guid.NewGuid(),
                StartTime = DateTime.UtcNow.AddHours(1),
                EndTime = default
            };

            var result = _validator.TestValidate(command);

            result.ShouldHaveValidationErrorFor(x => x.EndTime)
                  .WithErrorMessage("Sluttid måste anges.");
        }

        [Fact]
        public void Should_Have_Error_When_EndTime_Is_Before_StartTime()
        {
            var command = new UpdateShiftCommand
            {
                ShiftId = Guid.NewGuid(),
                StartTime = DateTime.UtcNow.AddHours(12),
                EndTime = DateTime.UtcNow.AddHours(10)
            };

            var result = _validator.TestValidate(command);

            result.ShouldHaveValidationErrorFor(x => x.EndTime)
                  .WithErrorMessage("Passet kan inte sluta innan det har börjat.");
        }
    }
}
