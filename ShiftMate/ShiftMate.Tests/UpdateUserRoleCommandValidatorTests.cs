using FluentValidation.TestHelper;
using ShiftMate.Application.Users.Commands;

namespace ShiftMate.Tests
{
    public class UpdateUserRoleCommandValidatorTests
    {
        private readonly UpdateUserRoleCommandValidator _validator = new();

        [Theory]
        [InlineData("Employee")]
        [InlineData("Manager")]
        public void Should_Not_Have_Errors_When_Valid(string role)
        {
            var command = new UpdateUserRoleCommand
            {
                TargetUserId = Guid.NewGuid(),
                NewRole = role,
                RequestingUserId = Guid.NewGuid(),
                OrganizationId = Guid.NewGuid()
            };

            var result = _validator.TestValidate(command);

            result.ShouldNotHaveAnyValidationErrors();
        }

        [Fact]
        public void Should_Have_Error_When_TargetUserId_Is_Empty()
        {
            var command = new UpdateUserRoleCommand
            {
                TargetUserId = Guid.Empty,
                NewRole = "Employee",
                RequestingUserId = Guid.NewGuid(),
                OrganizationId = Guid.NewGuid()
            };

            var result = _validator.TestValidate(command);

            result.ShouldHaveValidationErrorFor(x => x.TargetUserId)
                  .WithErrorMessage("Användar-ID krävs.");
        }

        [Theory]
        [InlineData("")]
        [InlineData(null)]
        public void Should_Have_Error_When_NewRole_Is_Empty(string role)
        {
            var command = new UpdateUserRoleCommand
            {
                TargetUserId = Guid.NewGuid(),
                NewRole = role,
                RequestingUserId = Guid.NewGuid(),
                OrganizationId = Guid.NewGuid()
            };

            var result = _validator.TestValidate(command);

            result.ShouldHaveValidationErrorFor(x => x.NewRole)
                  .WithErrorMessage("Roll måste anges.");
        }

        [Theory]
        [InlineData("SuperAdmin")]
        [InlineData("Admin")]
        [InlineData("InvalidRole")]
        public void Should_Have_Error_When_NewRole_Is_Invalid(string role)
        {
            var command = new UpdateUserRoleCommand
            {
                TargetUserId = Guid.NewGuid(),
                NewRole = role,
                RequestingUserId = Guid.NewGuid(),
                OrganizationId = Guid.NewGuid()
            };

            var result = _validator.TestValidate(command);

            result.ShouldHaveValidationErrorFor(x => x.NewRole)
                  .WithErrorMessage("Roll måste vara 'Employee' eller 'Manager'.");
        }
    }
}
