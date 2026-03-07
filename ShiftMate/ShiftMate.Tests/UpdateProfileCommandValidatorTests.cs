using FluentValidation.TestHelper;
using ShiftMate.Application.Users.Commands;

namespace ShiftMate.Tests
{
    public class UpdateProfileCommandValidatorTests
    {
        private readonly UpdateProfileCommandValidator _validator = new();

        [Fact]
        public void Should_Not_Have_Errors_When_Valid()
        {
            var command = new UpdateProfileCommand
            {
                UserId = Guid.NewGuid(),
                FirstName = "Anna",
                LastName = "Svensson",
                Email = "anna@test.com"
            };

            var result = _validator.TestValidate(command);

            result.ShouldNotHaveAnyValidationErrors();
        }

        [Theory]
        [InlineData("")]
        [InlineData(null)]
        public void Should_Have_Error_When_FirstName_Is_Empty(string firstName)
        {
            var command = new UpdateProfileCommand
            {
                FirstName = firstName,
                LastName = "Svensson",
                Email = "anna@test.com"
            };

            var result = _validator.TestValidate(command);

            result.ShouldHaveValidationErrorFor(x => x.FirstName)
                  .WithErrorMessage("Förnamn måste anges.");
        }

        [Fact]
        public void Should_Have_Error_When_FirstName_Exceeds_50_Characters()
        {
            var command = new UpdateProfileCommand
            {
                FirstName = new string('A', 51),
                LastName = "Svensson",
                Email = "anna@test.com"
            };

            var result = _validator.TestValidate(command);

            result.ShouldHaveValidationErrorFor(x => x.FirstName)
                  .WithErrorMessage("Förnamn får vara max 50 tecken.");
        }

        [Theory]
        [InlineData("")]
        [InlineData(null)]
        public void Should_Have_Error_When_LastName_Is_Empty(string lastName)
        {
            var command = new UpdateProfileCommand
            {
                FirstName = "Anna",
                LastName = lastName,
                Email = "anna@test.com"
            };

            var result = _validator.TestValidate(command);

            result.ShouldHaveValidationErrorFor(x => x.LastName)
                  .WithErrorMessage("Efternamn måste anges.");
        }

        [Fact]
        public void Should_Have_Error_When_LastName_Exceeds_50_Characters()
        {
            var command = new UpdateProfileCommand
            {
                FirstName = "Anna",
                LastName = new string('B', 51),
                Email = "anna@test.com"
            };

            var result = _validator.TestValidate(command);

            result.ShouldHaveValidationErrorFor(x => x.LastName)
                  .WithErrorMessage("Efternamn får vara max 50 tecken.");
        }

        [Theory]
        [InlineData("")]
        [InlineData(null)]
        public void Should_Have_Error_When_Email_Is_Empty(string email)
        {
            var command = new UpdateProfileCommand
            {
                FirstName = "Anna",
                LastName = "Svensson",
                Email = email
            };

            var result = _validator.TestValidate(command);

            result.ShouldHaveValidationErrorFor(x => x.Email)
                  .WithErrorMessage("E-postadress måste anges.");
        }

        [Fact]
        public void Should_Have_Error_When_Email_Is_Invalid()
        {
            var command = new UpdateProfileCommand
            {
                FirstName = "Anna",
                LastName = "Svensson",
                Email = "inte-en-epost"
            };

            var result = _validator.TestValidate(command);

            result.ShouldHaveValidationErrorFor(x => x.Email)
                  .WithErrorMessage("En giltig e-postadress krävs.");
        }
    }
}
