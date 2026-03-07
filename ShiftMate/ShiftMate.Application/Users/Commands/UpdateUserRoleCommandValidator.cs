using FluentValidation;

namespace ShiftMate.Application.Users.Commands
{
    public class UpdateUserRoleCommandValidator : AbstractValidator<UpdateUserRoleCommand>
    {
        public UpdateUserRoleCommandValidator()
        {
            RuleFor(x => x.TargetUserId)
                .NotEmpty().WithMessage("Användar-ID krävs.");

            RuleFor(x => x.NewRole)
                .NotEmpty().WithMessage("Roll måste anges.")
                .Must(role => role == "Employee" || role == "Manager")
                .WithMessage("Roll måste vara 'Employee' eller 'Manager'.");
        }
    }
}
