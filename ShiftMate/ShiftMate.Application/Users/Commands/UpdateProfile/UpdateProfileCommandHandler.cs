using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using ShiftMate.Application.Common.Exceptions;
using ShiftMate.Application.Interfaces;

namespace ShiftMate.Application.Users.Commands.UpdateProfile;

// Handlern för att uppdatera användarprofilen. Den validerar input, hämtar användaren från databasen, uppdaterar fälten och sparar ändringarna.
public class UpdateProfileCommandHandler : IRequestHandler<UpdateProfileCommand>
{
    private readonly IAppDbContext _context;
    private readonly IValidator<UpdateProfileCommand> _validator;

    public UpdateProfileCommandHandler(IAppDbContext context, IValidator<UpdateProfileCommand> validator)
    {
        _context = context;
        _validator = validator;
    }

    public async Task Handle(UpdateProfileCommand request, CancellationToken cancellationToken)
    {
        // 1. VALIDERING
        var validationResult = await _validator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            throw new ValidationException(validationResult.Errors);
        }

        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.Id == request.UserId, cancellationToken);

        if (user == null) throw new NotFoundException("Användaren hittades inte.");

        // Uppdatera fälten
        user.FirstName = request.FirstName;
        user.LastName = request.LastName;
        user.Email = request.Email;

        await _context.SaveChangesAsync(cancellationToken);
    }
}
