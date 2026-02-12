using FluentValidation;

namespace ShiftMate.Application.Users.Commands
{
    // Valideringsregler för lösenordsbyte
    public class ChangePasswordCommandValidator : AbstractValidator<ChangePasswordCommand>
    {
        public ChangePasswordCommandValidator()
        {
            // Regel: Nuvarande lösenord får inte vara tomt
            RuleFor(x => x.CurrentPassword)
                .NotEmpty().WithMessage("Nuvarande lösenord måste anges.");

            // Regel: Nytt lösenord får inte vara tomt och måste vara minst 8 tecken
            RuleFor(x => x.NewPassword)
                .NotEmpty().WithMessage("Nytt lösenord måste anges.")
                .MinimumLength(8).WithMessage("Lösenordet måste vara minst 8 tecken långt.");
        }
    }
}
