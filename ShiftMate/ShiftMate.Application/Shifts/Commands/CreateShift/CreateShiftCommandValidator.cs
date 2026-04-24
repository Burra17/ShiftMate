using FluentValidation;

namespace ShiftMate.Application.Shifts.Commands.CreateShift;

// Validator för CreateShiftCommand som säkerställer att starttid och sluttid är giltiga och följer logiska regler.
public class CreateShiftCommandValidator : AbstractValidator<CreateShiftCommand>
{
    public CreateShiftCommandValidator()
    {
        RuleFor(x => x.StartTime)
            .NotEmpty().WithMessage("Starttid måste anges.");

        RuleFor(x => x.EndTime)
            .NotEmpty().WithMessage("Sluttid måste anges.");

        RuleFor(x => x.EndTime)
            .GreaterThan(x => x.StartTime)
            .WithMessage("Passet kan inte sluta innan det har börjat.");

        RuleFor(x => x.StartTime)
            .GreaterThan(DateTime.UtcNow)
            .WithMessage("Du kan inte skapa pass i dåtiden.");
    }
}