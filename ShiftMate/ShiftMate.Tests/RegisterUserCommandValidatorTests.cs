using FluentValidation.TestHelper;
using ShiftMate.Application.Users.Commands;

namespace ShiftMate.Tests
{
    public class RegisterUserCommandValidatorTests
    {
        private readonly RegisterUserCommandValidator _validator = new();

        [Fact]
        public void Should_Not_Have_Errors_When_Valid()
        {
            var command = new RegisterUserCommand(
                "Anna", "Svensson", "anna@test.com", "password123", "ABC12345");

            var result = _validator.TestValidate(command);

            result.ShouldNotHaveAnyValidationErrors();
        }

        [Theory]
        [InlineData("")]
        [InlineData(null)]
        public void Should_Have_Error_When_FirstName_Is_Empty(string firstName)
        {
            var command = new RegisterUserCommand(
                firstName, "Svensson", "anna@test.com", "password123", "ABC12345");

            var result = _validator.TestValidate(command);

            result.ShouldHaveValidationErrorFor(x => x.FirstName)
                  .WithErrorMessage("Förnamn måste anges.");
        }

        [Theory]
        [InlineData("")]
        [InlineData(null)]
        public void Should_Have_Error_When_LastName_Is_Empty(string lastName)
        {
            var command = new RegisterUserCommand(
                "Anna", lastName, "anna@test.com", "password123", "ABC12345");

            var result = _validator.TestValidate(command);

            result.ShouldHaveValidationErrorFor(x => x.LastName)
                  .WithErrorMessage("Efternamn måste anges.");
        }

        [Theory]
        [InlineData("")]
        [InlineData(null)]
        public void Should_Have_Error_When_Email_Is_Empty(string email)
        {
            var command = new RegisterUserCommand(
                "Anna", "Svensson", email, "password123", "ABC12345");

            var result = _validator.TestValidate(command);

            result.ShouldHaveValidationErrorFor(x => x.Email)
                  .WithErrorMessage("E-postadress måste anges.");
        }

        [Fact]
        public void Should_Have_Error_When_Email_Is_Invalid()
        {
            var command = new RegisterUserCommand(
                "Anna", "Svensson", "inte-giltig", "password123", "ABC12345");

            var result = _validator.TestValidate(command);

            result.ShouldHaveValidationErrorFor(x => x.Email)
                  .WithErrorMessage("En giltig e-postadress krävs.");
        }

        [Theory]
        [InlineData("")]
        [InlineData(null)]
        public void Should_Have_Error_When_Password_Is_Empty(string password)
        {
            var command = new RegisterUserCommand(
                "Anna", "Svensson", "anna@test.com", password, "ABC12345");

            var result = _validator.TestValidate(command);

            result.ShouldHaveValidationErrorFor(x => x.Password)
                  .WithErrorMessage("Lösenord måste anges.");
        }

        [Fact]
        public void Should_Have_Error_When_Password_Is_Too_Short()
        {
            var command = new RegisterUserCommand(
                "Anna", "Svensson", "anna@test.com", "short", "ABC12345");

            var result = _validator.TestValidate(command);

            result.ShouldHaveValidationErrorFor(x => x.Password)
                  .WithErrorMessage("Lösenordet måste vara minst 8 tecken långt.");
        }

        [Theory]
        [InlineData("")]
        [InlineData(null)]
        public void Should_Have_Error_When_InviteCode_Is_Empty(string inviteCode)
        {
            var command = new RegisterUserCommand(
                "Anna", "Svensson", "anna@test.com", "password123", inviteCode);

            var result = _validator.TestValidate(command);

            result.ShouldHaveValidationErrorFor(x => x.InviteCode)
                  .WithErrorMessage("Inbjudningskod måste anges.");
        }

        [Theory]
        [InlineData("SHORT")]
        [InlineData("TOOLONGCODE")]
        public void Should_Have_Error_When_InviteCode_Is_Not_8_Characters(string inviteCode)
        {
            var command = new RegisterUserCommand(
                "Anna", "Svensson", "anna@test.com", "password123", inviteCode);

            var result = _validator.TestValidate(command);

            result.ShouldHaveValidationErrorFor(x => x.InviteCode)
                  .WithErrorMessage("Inbjudningskoden måste vara 8 tecken.");
        }
    }
}
