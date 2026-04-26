using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using ShiftMate.Application.Interfaces;

namespace ShiftMate.Application.Users.Commands.ChangePassword;

// Handlern för att byta lösenord. Den validerar det nuvarande lösenordet, hashar det nya och uppdaterar användarens lösenord i databasen.
public class ChangePasswordCommandHandler : IRequestHandler<ChangePasswordCommand>
{
    private readonly IAppDbContext _context;
    private readonly IValidator<ChangePasswordCommand> _validator;

    public ChangePasswordCommandHandler(IAppDbContext context, IValidator<ChangePasswordCommand> validator)
    {
        _context = context;
        _validator = validator;
    }

    public async Task Handle(ChangePasswordCommand request, CancellationToken cancellationToken)
    {
        // 1. VALIDERING
        var validationResult = await _validator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            throw new ValidationException(validationResult.Errors);
        }

        // 2. Hämta användaren från databasen
        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.Id == request.UserId, cancellationToken);

        if (user == null)
            throw new Exception("Användaren hittades inte.");

        // 3. Verifiera att nuvarande lösenord stämmer
        if (!BCrypt.Net.BCrypt.Verify(request.CurrentPassword, user.PasswordHash))
            throw new Exception("Nuvarande lösenord är felaktigt.");

        // 4. Hasha och spara det nya lösenordet
        user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.NewPassword);
        await _context.SaveChangesAsync(cancellationToken);
    }
}
