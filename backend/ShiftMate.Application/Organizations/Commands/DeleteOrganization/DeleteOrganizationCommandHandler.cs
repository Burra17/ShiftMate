using MediatR;
using Microsoft.EntityFrameworkCore;
using ShiftMate.Application.Common.Exceptions;
using ShiftMate.Application.Interfaces;

namespace ShiftMate.Application.Organizations.Commands.DeleteOrganization;

// Command handler för att radera en organisation och alla dess relaterade data (användare, pass, bytesförfrågningar).
public class DeleteOrganizationCommandHandler : IRequestHandler<DeleteOrganizationCommand>
{
    private readonly IAppDbContext _context;

    public DeleteOrganizationCommandHandler(IAppDbContext context)
    {
        _context = context;
    }

    public async Task Handle(DeleteOrganizationCommand request, CancellationToken cancellationToken)
    {
        var exists = await _context.Organizations.AnyAsync(o => o.Id == request.Id, cancellationToken);

        if (!exists)
        {
            throw new NotFoundException("Organisationen hittades inte.");
        }

        await OrganizationRemover.RemoveWithAllDataAsync(_context, [request.Id], cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
    }
}
