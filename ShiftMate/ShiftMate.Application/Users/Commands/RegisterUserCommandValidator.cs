using FluentValidation;

namespace ShiftMate.Application.Users.Commands
{
    public class RegisterUserCommandValidator : AbstractValidator<RegisterUserCommand>
    {
        public RegisterUserCommandValidator()
        {
            // Regel: Förnamn får inte vara tomt.
            RuleFor(x => x.FirstName)
                .NotEmpty().WithMessage("Förnamn måste anges.");

            // Regel: Efternamn får inte vara tomt.
            RuleFor(x => x.LastName)
                .NotEmpty().WithMessage("Efternamn måste anges.");

            // Regel: E-post får inte vara tomt och måste vara en giltig e-postadress.
            RuleFor(x => x.Email)
                .NotEmpty().WithMessage("E-postadress måste anges.")
                .EmailAddress().WithMessage("En giltig e-postadress krävs.");

            // Regel: Lösenord får inte vara tomt och måste vara minst 8 tecken långt.
            RuleFor(x => x.Password)
                .NotEmpty().WithMessage("Lösenord måste anges.")
                .MinimumLength(8).WithMessage("Lösenordet måste vara minst 8 tecken långt.");
        }
    }
}
