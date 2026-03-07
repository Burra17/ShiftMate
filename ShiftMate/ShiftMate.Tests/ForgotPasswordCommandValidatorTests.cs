using FluentValidation.TestHelper;
using ShiftMate.Application.Users.Commands;

namespace ShiftMate.Tests
{
    public class ForgotPasswordCommandValidatorTests
    {
        private readonly ForgotPasswordCommandValidator _validator = new();

        [Fact]
        public void Should_Not_Have_Errors_When_Valid()
        {
            var command = new ForgotPasswordCommand { Email = "user@test.com" };

            var result = _validator.TestValidate(command);

            result.ShouldNotHaveAnyValidationErrors();
        }

        [Theory]
        [InlineData("")]
        [InlineData(null)]
        public void Should_Have_Error_When_Email_Is_Empty(string email)
        {
            var command = new ForgotPasswordCommand { Email = email };

            var result = _validator.TestValidate(command);

            result.ShouldHaveValidationErrorFor(x => x.Email)
                  .WithErrorMessage("E-post måste anges.");
        }

        [Fact]
        public void Should_Have_Error_When_Email_Is_Invalid()
        {
            var command = new ForgotPasswordCommand { Email = "inte-giltig" };

            var result = _validator.TestValidate(command);

            result.ShouldHaveValidationErrorFor(x => x.Email)
                  .WithErrorMessage("Ogiltig e-postadress.");
        }
    }
}
