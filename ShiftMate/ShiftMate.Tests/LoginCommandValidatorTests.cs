using FluentValidation.TestHelper;
using ShiftMate.Application.Users.Commands;

namespace ShiftMate.Tests
{
    public class LoginCommandValidatorTests
    {
        private readonly LoginCommandValidator _validator = new();

        [Fact]
        public void Should_Not_Have_Errors_When_Valid()
        {
            var command = new LoginCommand
            {
                Email = "user@test.com",
                Password = "password123"
            };

            var result = _validator.TestValidate(command);

            result.ShouldNotHaveAnyValidationErrors();
        }

        [Theory]
        [InlineData("")]
        [InlineData(null)]
        public void Should_Have_Error_When_Email_Is_Empty(string email)
        {
            var command = new LoginCommand
            {
                Email = email,
                Password = "password123"
            };

            var result = _validator.TestValidate(command);

            result.ShouldHaveValidationErrorFor(x => x.Email)
                  .WithErrorMessage("E-postadress måste anges.");
        }

        [Fact]
        public void Should_Have_Error_When_Email_Is_Invalid()
        {
            var command = new LoginCommand
            {
                Email = "inte-giltig",
                Password = "password123"
            };

            var result = _validator.TestValidate(command);

            result.ShouldHaveValidationErrorFor(x => x.Email)
                  .WithErrorMessage("En giltig e-postadress krävs.");
        }

        [Theory]
        [InlineData("")]
        [InlineData(null)]
        public void Should_Have_Error_When_Password_Is_Empty(string password)
        {
            var command = new LoginCommand
            {
                Email = "user@test.com",
                Password = password
            };

            var result = _validator.TestValidate(command);

            result.ShouldHaveValidationErrorFor(x => x.Password)
                  .WithErrorMessage("Lösenord måste anges.");
        }
    }
}
