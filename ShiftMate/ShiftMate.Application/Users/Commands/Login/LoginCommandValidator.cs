using FluentValidation;

namespace ShiftMate.Application.Users.Commands.Login;

// Valideringsregler för inloggning
public class LoginCommandValidator : AbstractValidator<LoginCommand>
{
    public LoginCommandValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("E-postadress måste anges.")
            .EmailAddress().WithMessage("En giltig e-postadress krävs.");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Lösenord måste anges.");
    }
}
