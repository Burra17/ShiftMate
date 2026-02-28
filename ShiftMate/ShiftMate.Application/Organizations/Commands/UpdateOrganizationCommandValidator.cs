using FluentValidation;

namespace ShiftMate.Application.Organizations.Commands
{
    public class UpdateOrganizationCommandValidator : AbstractValidator<UpdateOrganizationCommand>
    {
        public UpdateOrganizationCommandValidator()
        {
            RuleFor(x => x.Id)
                .NotEmpty().WithMessage("Organisations-ID krävs.");

            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("Organisationsnamn krävs.")
                .MaximumLength(100).WithMessage("Organisationsnamn får vara max 100 tecken.");
        }
    }
}
