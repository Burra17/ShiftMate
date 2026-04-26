using FluentValidation;

namespace ShiftMate.Application.Users.Commands.RegisterUser;

// Valideringsregler för registrering av ny användare
public class RegisterUserCommandValidator : AbstractValidator<RegisterUserCommand>
{
    public RegisterUserCommandValidator()
    {
        RuleFor(x => x.FirstName)
            .NotEmpty().WithMessage("Förnamn måste anges.");

        RuleFor(x => x.LastName)
            .NotEmpty().WithMessage("Efternamn måste anges.");

        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("E-postadress måste anges.")
            .EmailAddress().WithMessage("En giltig e-postadress krävs.");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Lösenord måste anges.")
            .MinimumLength(8).WithMessage("Lösenordet måste vara minst 8 tecken långt.");

        RuleFor(x => x.InviteCode)
            .NotEmpty().WithMessage("Inbjudningskod måste anges.")
            .Length(8).WithMessage("Inbjudningskoden måste vara 8 tecken.");
    }
}
