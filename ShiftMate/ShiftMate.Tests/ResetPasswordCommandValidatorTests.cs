using FluentValidation.TestHelper;
using ShiftMate.Application.Users.Commands.ResetPassword;

namespace ShiftMate.Tests
{
    public class ResetPasswordCommandValidatorTests
    {
        private readonly ResetPasswordCommandValidator _validator = new();

        [Fact]
        public void Should_Not_Have_Errors_When_Valid()
        {
            var command = new ResetPasswordCommand
            {
                Token = "valid-token-string",
                Email = "user@test.com",
                NewPassword = "newpassword123"
            };

            var result = _validator.TestValidate(command);

            result.ShouldNotHaveAnyValidationErrors();
        }

        [Theory]
        [InlineData("")]
        [InlineData(null)]
        public void Should_Have_Error_When_Token_Is_Empty(string token)
        {
            var command = new ResetPasswordCommand
            {
                Token = token,
                Email = "user@test.com",
                NewPassword = "newpassword123"
            };

            var result = _validator.TestValidate(command);

            result.ShouldHaveValidationErrorFor(x => x.Token)
                  .WithErrorMessage("Token saknas.");
        }

        [Theory]
        [InlineData("")]
        [InlineData(null)]
        public void Should_Have_Error_When_Email_Is_Empty(string email)
        {
            var command = new ResetPasswordCommand
            {
                Token = "valid-token",
                Email = email,
                NewPassword = "newpassword123"
            };

            var result = _validator.TestValidate(command);

            result.ShouldHaveValidationErrorFor(x => x.Email)
                  .WithErrorMessage("E-post måste anges.");
        }

        [Fact]
        public void Should_Have_Error_When_Email_Is_Invalid()
        {
            var command = new ResetPasswordCommand
            {
                Token = "valid-token",
                Email = "inte-giltig",
                NewPassword = "newpassword123"
            };

            var result = _validator.TestValidate(command);

            result.ShouldHaveValidationErrorFor(x => x.Email)
                  .WithErrorMessage("Ogiltig e-postadress.");
        }

        [Theory]
        [InlineData("")]
        [InlineData(null)]
        public void Should_Have_Error_When_NewPassword_Is_Empty(string password)
        {
            var command = new ResetPasswordCommand
            {
                Token = "valid-token",
                Email = "user@test.com",
                NewPassword = password
            };

            var result = _validator.TestValidate(command);

            result.ShouldHaveValidationErrorFor(x => x.NewPassword)
                  .WithErrorMessage("Lösenord måste anges.");
        }

        [Fact]
        public void Should_Have_Error_When_NewPassword_Is_Too_Short()
        {
            var command = new ResetPasswordCommand
            {
                Token = "valid-token",
                Email = "user@test.com",
                NewPassword = "short"
            };

            var result = _validator.TestValidate(command);

            result.ShouldHaveValidationErrorFor(x => x.NewPassword)
                  .WithErrorMessage("Lösenordet måste vara minst 8 tecken.");
        }
    }
}
