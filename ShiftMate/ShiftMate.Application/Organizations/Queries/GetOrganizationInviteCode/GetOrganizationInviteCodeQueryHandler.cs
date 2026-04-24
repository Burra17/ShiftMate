using MediatR;
using Microsoft.EntityFrameworkCore;
using ShiftMate.Application.Interfaces;

namespace ShiftMate.Application.Organizations.Queries.GetOrganizationInviteCode;

// Query handler för att hämta en organisations invite code, namn och när den genererades.
public class GetOrganizationInviteCodeQueryHandler : IRequestHandler<GetOrganizationInviteCodeQuery, InviteCodeResult>
{
    private readonly IAppDbContext _context;

    public GetOrganizationInviteCodeQueryHandler(IAppDbContext context)
    {
        _context = context;
    }

    public async Task<InviteCodeResult> Handle(GetOrganizationInviteCodeQuery request, CancellationToken cancellationToken)
    {
        var organization = await _context.Organizations
            .AsNoTracking()
            .FirstOrDefaultAsync(o => o.Id == request.OrganizationId, cancellationToken);

        if (organization == null)
        {
            throw new InvalidOperationException("Organisationen hittades inte.");
        }

        return new InviteCodeResult(organization.InviteCode, organization.Name, organization.InviteCodeGeneratedAt);
    }
}