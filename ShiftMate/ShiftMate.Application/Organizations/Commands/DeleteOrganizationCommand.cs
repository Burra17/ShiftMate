using MediatR;
using Microsoft.EntityFrameworkCore;
using ShiftMate.Application.Interfaces;

namespace ShiftMate.Application.Organizations.Commands
{
    public record DeleteOrganizationCommand(Guid Id) : IRequest;

    public class DeleteOrganizationHandler : IRequestHandler<DeleteOrganizationCommand>
    {
        private readonly IAppDbContext _context;

        public DeleteOrganizationHandler(IAppDbContext context)
        {
            _context = context;
        }

        public async Task Handle(DeleteOrganizationCommand request, CancellationToken cancellationToken)
        {
            var organization = await _context.Organizations
                .Include(o => o.Users)
                .Include(o => o.Shifts)
                .FirstOrDefaultAsync(o => o.Id == request.Id, cancellationToken);

            if (organization == null)
            {
                throw new Exception("Organisationen hittades inte.");
            }

            // Radera i FK-ordning: förfrågningar → pass → användare → organisation
            var userIds = organization.Users.Select(u => u.Id).ToList();
            var shiftIds = organization.Shifts.Select(s => s.Id).ToList();

            if (shiftIds.Any() || userIds.Any())
            {
                // Ta bort alla bytesförfrågningar kopplade till organisationens pass eller användare
                var swapRequests = await _context.SwapRequests
                    .Where(sr => shiftIds.Contains(sr.ShiftId)
                        || userIds.Contains(sr.RequestingUserId)
                        || (sr.TargetUserId != null && userIds.Contains(sr.TargetUserId.Value)))
                    .ToListAsync(cancellationToken);

                _context.SwapRequests.RemoveRange(swapRequests);
            }

            _context.Shifts.RemoveRange(organization.Shifts);
            _context.Users.RemoveRange(organization.Users);
            _context.Organizations.Remove(organization);

            await _context.SaveChangesAsync(cancellationToken);
        }
    }
}
