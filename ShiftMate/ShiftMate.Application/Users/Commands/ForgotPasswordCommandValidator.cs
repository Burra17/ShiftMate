using FluentValidation;

namespace ShiftMate.Application.Users.Commands
{
    public class ForgotPasswordCommandValidator : AbstractValidator<ForgotPasswordCommand>
    {
        public ForgotPasswordCommandValidator()
        {
            RuleFor(x => x.Email)
                .NotEmpty().WithMessage("E-post måste anges.")
                .EmailAddress().WithMessage("Ogiltig e-postadress.");
        }
    }
}
