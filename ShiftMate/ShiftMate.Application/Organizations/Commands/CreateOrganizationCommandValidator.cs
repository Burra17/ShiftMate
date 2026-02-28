using FluentValidation;

namespace ShiftMate.Application.Organizations.Commands
{
    public class CreateOrganizationCommandValidator : AbstractValidator<CreateOrganizationCommand>
    {
        public CreateOrganizationCommandValidator()
        {
            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("Organisationsnamn krävs.")
                .MaximumLength(100).WithMessage("Organisationsnamn får vara max 100 tecken.");
        }
    }
}
