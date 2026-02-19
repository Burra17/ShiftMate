using FluentValidation;

namespace ShiftMate.Application.Shifts.Commands
{
    // Validator för UpdateShiftCommand — samma regler som create, men utan "måste vara i framtiden"
    // så att managers kan korrigera redan passerade pass
    public class UpdateShiftCommandValidator : AbstractValidator<UpdateShiftCommand>
    {
        public UpdateShiftCommandValidator()
        {
            // Regel 1: Starttid får inte vara tom
            RuleFor(x => x.StartTime)
                .NotEmpty().WithMessage("Starttid måste anges.");

            // Regel 2: Sluttid får inte vara tom
            RuleFor(x => x.EndTime)
                .NotEmpty().WithMessage("Sluttid måste anges.");

            // Regel 3: Sluttid måste vara EFTER starttid
            RuleFor(x => x.EndTime)
                .GreaterThan(x => x.StartTime)
                .WithMessage("Passet kan inte sluta innan det har börjat.");
        }
    }
}
