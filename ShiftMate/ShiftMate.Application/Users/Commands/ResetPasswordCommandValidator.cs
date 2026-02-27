using FluentValidation;

namespace ShiftMate.Application.Users.Commands
{
    public class ResetPasswordCommandValidator : AbstractValidator<ResetPasswordCommand>
    {
        public ResetPasswordCommandValidator()
        {
            RuleFor(x => x.Token)
                .NotEmpty().WithMessage("Token saknas.");

            RuleFor(x => x.Email)
                .NotEmpty().WithMessage("E-post måste anges.")
                .EmailAddress().WithMessage("Ogiltig e-postadress.");

            RuleFor(x => x.NewPassword)
                .NotEmpty().WithMessage("Lösenord måste anges.")
                .MinimumLength(8).WithMessage("Lösenordet måste vara minst 8 tecken.");
        }
    }
}
