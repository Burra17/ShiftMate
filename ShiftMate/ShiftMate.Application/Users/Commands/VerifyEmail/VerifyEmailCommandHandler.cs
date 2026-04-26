using MediatR;
using Microsoft.EntityFrameworkCore;
using ShiftMate.Application.Interfaces;

namespace ShiftMate.Application.Users.Commands.VerifyEmail;

// Handlern för att verifiera en e-postadress. Den validerar token, kontrollerar utgångstid och uppdaterar användarens verifieringsstatus i databasen.
public class VerifyEmailCommandHandler : IRequestHandler<VerifyEmailCommand>
{
    private readonly IAppDbContext _context;

    public VerifyEmailCommandHandler(IAppDbContext context)
    {
        _context = context;
    }

    public async Task Handle(VerifyEmailCommand request, CancellationToken cancellationToken)
    {
        const string errorMessage = "Ogiltig eller utgången verifieringslänk.";

        // 1. Hitta användaren
        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.Email == request.Email.ToLowerInvariant(), cancellationToken);

        if (user == null)
            throw new Exception(errorMessage);

        // 2. Redan verifierad?
        if (user.IsEmailVerified)
            throw new InvalidOperationException("E-postadressen är redan verifierad. Du kan logga in.");

        // 3. Kontrollera att det finns en aktiv token
        if (string.IsNullOrEmpty(user.EmailVerificationTokenHash))
            throw new Exception(errorMessage);

        // 4. Kontrollera utgångstid
        if (user.EmailVerificationTokenExpiresAt == null || user.EmailVerificationTokenExpiresAt < DateTime.UtcNow)
            throw new Exception(errorMessage);

        // 5. Verifiera token mot lagrad hash
        if (!BCrypt.Net.BCrypt.Verify(request.Token, user.EmailVerificationTokenHash))
            throw new Exception(errorMessage);

        // 6. Markera som verifierad och rensa token
        user.IsEmailVerified = true;
        user.EmailVerificationTokenHash = null;
        user.EmailVerificationTokenExpiresAt = null;
        await _context.SaveChangesAsync(cancellationToken);
    }
}
