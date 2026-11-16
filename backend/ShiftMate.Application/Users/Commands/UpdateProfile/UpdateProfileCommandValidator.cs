using FluentValidation;

namespace ShiftMate.Application.Users.Commands.UpdateProfile;

// Valideringsregler för uppdatering av användarprofil
public class UpdateProfileCommandValidator : AbstractValidator<UpdateProfileCommand>
{
    public UpdateProfileCommandValidator()
    {
        RuleFor(x => x.FirstName)
            .NotEmpty().WithMessage("Förnamn måste anges.")
            .MaximumLength(50).WithMessage("Förnamn får vara max 50 tecken.");

        RuleFor(x => x.LastName)
            .NotEmpty().WithMessage("Efternamn måste anges.")
            .MaximumLength(50).WithMessage("Efternamn får vara max 50 tecken.");

        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("E-postadress måste anges.")
            .EmailAddress().WithMessage("En giltig e-postadress krävs.");
    }
}
