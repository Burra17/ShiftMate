using FluentValidation;

namespace ShiftMate.Application.Organizations.Commands.CreateOrganization;

// Validator för CreateOrganizationCommand som säkerställer att det angivna namnet är giltigt.
public class CreateOrganizationCommandValidator : AbstractValidator<CreateOrganizationCommand>
{
    public CreateOrganizationCommandValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Organisationsnamn krävs.")
            .MaximumLength(100).WithMessage("Organisationsnamn får vara max 100 tecken.");
    }
}
