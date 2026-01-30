using FluentValidation;

namespace ShiftMate.Application.Shifts.Commands
{
    // Vi skapar en "Validator" för vårt CreateShiftCommand
    public class CreateShiftCommandValidator : AbstractValidator<CreateShiftCommand>
    {
        public CreateShiftCommandValidator()
        {
            // Regel 1: Starttid får inte vara tom
            RuleFor(x => x.StartTime)
                .NotEmpty().WithMessage("Starttid måste anges.");

            // Regel 2: Sluttid får inte vara tom
            RuleFor(x => x.EndTime)
                .NotEmpty().WithMessage("Sluttid måste anges.");

            // Regel 3: Sluttid måste vara EFTER starttid (Logiskt!)
            RuleFor(x => x.EndTime)
                .GreaterThan(x => x.StartTime)
                .WithMessage("Passet kan inte sluta innan det har börjat.");

            // Regel 4: Passet måste vara i framtiden (Vi kan inte jobba igår)
            RuleFor(x => x.StartTime)
                .GreaterThan(DateTime.UtcNow)
                .WithMessage("Du kan inte skapa pass i dåtiden.");
        }
    }
}