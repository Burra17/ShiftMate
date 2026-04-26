using MediatR;
using Microsoft.EntityFrameworkCore;
using ShiftMate.Application.Interfaces;

namespace ShiftMate.Application.Organizations.Commands.RegenerateInviteCode;

// Command handler för att regenerera en organisations invite code.
public class RegenerateInviteCodeCommandHandler : IRequestHandler<RegenerateInviteCodeCommand, string>
{
    private readonly IAppDbContext _context;

    public RegenerateInviteCodeCommandHandler(IAppDbContext context)
    {
        _context = context;
    }

    public async Task<string> Handle(RegenerateInviteCodeCommand request, CancellationToken cancellationToken)
    {
        var organization = await _context.Organizations
            .FirstOrDefaultAsync(o => o.Id == request.OrganizationId, cancellationToken);

        if (organization == null)
        {
            throw new InvalidOperationException("Organisationen hittades inte.");
        }

        organization.InviteCode = InviteCodeGenerator.GenerateInviteCode();
        organization.InviteCodeGeneratedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);

        return organization.InviteCode;
    }
}
