using FluentValidation.TestHelper;
using ShiftMate.Application.Users.Commands.ChangePassword;

namespace ShiftMate.Tests
{
    public class ChangePasswordCommandValidatorTests
    {
        private readonly ChangePasswordCommandValidator _validator = new();

        [Fact]
        public void Should_Not_Have_Errors_When_Valid()
        {
            var command = new ChangePasswordCommand
            {
                CurrentPassword = "oldpassword",
                NewPassword = "newpassword123"
            };

            var result = _validator.TestValidate(command);

            result.ShouldNotHaveAnyValidationErrors();
        }

        [Theory]
        [InlineData("")]
        [InlineData(null)]
        public void Should_Have_Error_When_CurrentPassword_Is_Empty(string currentPassword)
        {
            var command = new ChangePasswordCommand
            {
                CurrentPassword = currentPassword,
                NewPassword = "newpassword123"
            };

            var result = _validator.TestValidate(command);

            result.ShouldHaveValidationErrorFor(x => x.CurrentPassword)
                  .WithErrorMessage("Nuvarande lösenord måste anges.");
        }

        [Theory]
        [InlineData("")]
        [InlineData(null)]
        public void Should_Have_Error_When_NewPassword_Is_Empty(string newPassword)
        {
            var command = new ChangePasswordCommand
            {
                CurrentPassword = "oldpassword",
                NewPassword = newPassword
            };

            var result = _validator.TestValidate(command);

            result.ShouldHaveValidationErrorFor(x => x.NewPassword)
                  .WithErrorMessage("Nytt lösenord måste anges.");
        }

        [Fact]
        public void Should_Have_Error_When_NewPassword_Is_Too_Short()
        {
            var command = new ChangePasswordCommand
            {
                CurrentPassword = "oldpassword",
                NewPassword = "short"
            };

            var result = _validator.TestValidate(command);

            result.ShouldHaveValidationErrorFor(x => x.NewPassword)
                  .WithErrorMessage("Lösenordet måste vara minst 8 tecken långt.");
        }
    }
}
