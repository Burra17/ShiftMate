using MediatR;
using Microsoft.EntityFrameworkCore;
using ShiftMate.Application.Interfaces;

namespace ShiftMate.Application.Organizations.Queries
{
    public record GetOrganizationInviteCodeQuery(Guid OrganizationId) : IRequest<InviteCodeResult>;

    public record InviteCodeResult(string InviteCode, string OrganizationName, DateTime GeneratedAt);

    public class GetOrganizationInviteCodeHandler : IRequestHandler<GetOrganizationInviteCodeQuery, InviteCodeResult>
    {
        private readonly IAppDbContext _context;

        public GetOrganizationInviteCodeHandler(IAppDbContext context)
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
}
