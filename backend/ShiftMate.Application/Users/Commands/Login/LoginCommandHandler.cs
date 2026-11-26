using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using ShiftMate.Application.Common.Exceptions;
using ShiftMate.Application.Interfaces;
using ShiftMate.Domain.Enums;

namespace ShiftMate.Application.Users.Commands.Login;

// Handlern för inloggning.
// Den validerar användarens e-post och lösenord, kontrollerar kontostatus och e-postverifiering, och låter IJwtTokenGenerator skapa token.
public class LoginCommandHandler : IRequestHandler<LoginCommand, string>
{
    private readonly IAppDbContext _context;
    private readonly IJwtTokenGenerator _tokenGenerator;
    private readonly IValidator<LoginCommand> _validator;

    public LoginCommandHandler(IAppDbContext context, IJwtTokenGenerator tokenGenerator, IValidator<LoginCommand> validator)
    {
        _context = context;
        _tokenGenerator = tokenGenerator;
        _validator = validator;
    }

    public async Task<string> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        // 1. VALIDERING
        var validationResult = await _validator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            throw new ValidationException(validationResult.Errors);
        }

        // A. Hitta användaren (skiftlägesokänsligt) med organisation
        var user = await _context.Users
            .Include(u => u.Organization)
            .FirstOrDefaultAsync(u => u.Email == request.Email.ToLowerInvariant(), cancellationToken);

        // B. Validera lösenord med BCrypt
        if (user == null || !BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
        {
            throw new InvalidOperationException("Fel e-post eller lösenord.");
        }

        // B2. Kontrollera om kontot är inaktiverat
        if (!user.IsActive)
        {
            throw new InvalidOperationException("Ditt konto har inaktiverats. Kontakta din chef för mer information.");
        }

        // C. Kontrollera e-postverifiering (SuperAdmin undantas)
        if (!user.IsEmailVerified && user.Role != Role.SuperAdmin)
        {
            throw new EmailNotVerifiedException("E-postadressen är inte verifierad. Kontrollera din inkorg för verifieringslänken.");
        }

        // D. Skapa token (användaren är laddad med Organization ovan)
        return _tokenGenerator.GenerateToken(user);
    }
}
