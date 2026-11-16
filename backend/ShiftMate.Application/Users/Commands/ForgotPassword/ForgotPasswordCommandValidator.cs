using FluentValidation;

namespace ShiftMate.Application.Users.Commands.ForgotPassword;

// Valideringsregler för glömt lösenord
public class ForgotPasswordCommandValidator : AbstractValidator<ForgotPasswordCommand>
{
    public ForgotPasswordCommandValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("E-post måste anges.")
            .EmailAddress().WithMessage("Ogiltig e-postadress.");
    }
}
