using FluentValidation;

namespace ShiftMate.Application.Users.Commands
{
    public class RegisterUserCommandValidator : AbstractValidator<RegisterUserCommand>
    {
        public RegisterUserCommandValidator()
        {
            // Regel: Förnamn får inte vara tomt.
            RuleFor(x => x.FirstName)
                .NotEmpty().WithMessage("First name is required.");

            // Regel: Efternamn får inte vara tomt.
            RuleFor(x => x.LastName)
                .NotEmpty().WithMessage("Last name is required.");

            // Regel: E-post får inte vara tomt och måste vara en giltig e-postadress.
            RuleFor(x => x.Email)
                .NotEmpty().WithMessage("Email is required.")
                .EmailAddress().WithMessage("A valid email address is required.");

            // Regel: Lösenord får inte vara tomt och måste vara minst 8 tecken långt.
            RuleFor(x => x.Password)
                .NotEmpty().WithMessage("Password is required.")
                .MinimumLength(8).WithMessage("Password must be at least 8 characters long.");
        }
    }
}
