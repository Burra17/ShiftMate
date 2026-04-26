using FluentValidation;

namespace ShiftMate.Application.Users.Commands.ChangePassword;

// Valideringsregler för lösenordsbyte
public class ChangePasswordCommandValidator : AbstractValidator<ChangePasswordCommand>
{
    public ChangePasswordCommandValidator()
    {
        RuleFor(x => x.CurrentPassword)
            .NotEmpty().WithMessage("Nuvarande lösenord måste anges.");

        RuleFor(x => x.NewPassword)
            .NotEmpty().WithMessage("Nytt lösenord måste anges.")
            .MinimumLength(8).WithMessage("Lösenordet måste vara minst 8 tecken långt.");
    }
}
