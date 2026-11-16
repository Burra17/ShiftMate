using FluentValidation;

namespace ShiftMate.Application.Organizations.Commands.UpdateOrganization;

// Validator för UpdateOrganizationCommand som säkerställer att nödvändiga fält är ifyllda och att de följer vissa regler.
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
