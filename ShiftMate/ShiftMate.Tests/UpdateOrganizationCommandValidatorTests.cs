using FluentValidation.TestHelper;
using ShiftMate.Application.Organizations.Commands;

namespace ShiftMate.Tests
{
    public class UpdateOrganizationCommandValidatorTests
    {
        private readonly UpdateOrganizationCommandValidator _validator = new();

        [Fact]
        public void Should_Not_Have_Errors_When_Valid()
        {
            var command = new UpdateOrganizationCommand(Guid.NewGuid(), "Uppdaterad Organisation");

            var result = _validator.TestValidate(command);

            result.ShouldNotHaveAnyValidationErrors();
        }

        [Fact]
        public void Should_Have_Error_When_Id_Is_Empty()
        {
            var command = new UpdateOrganizationCommand(Guid.Empty, "Test");

            var result = _validator.TestValidate(command);

            result.ShouldHaveValidationErrorFor(x => x.Id)
                  .WithErrorMessage("Organisations-ID krävs.");
        }

        [Theory]
        [InlineData("")]
        [InlineData(null)]
        public void Should_Have_Error_When_Name_Is_Empty(string name)
        {
            var command = new UpdateOrganizationCommand(Guid.NewGuid(), name);

            var result = _validator.TestValidate(command);

            result.ShouldHaveValidationErrorFor(x => x.Name)
                  .WithErrorMessage("Organisationsnamn krävs.");
        }

        [Fact]
        public void Should_Have_Error_When_Name_Exceeds_100_Characters()
        {
            var command = new UpdateOrganizationCommand(Guid.NewGuid(), new string('A', 101));

            var result = _validator.TestValidate(command);

            result.ShouldHaveValidationErrorFor(x => x.Name)
                  .WithErrorMessage("Organisationsnamn får vara max 100 tecken.");
        }
    }
}
