using Microsoft.EntityFrameworkCore;
using ShiftMate.Application.Interfaces;

namespace ShiftMate.Application.Organizations;

// Markerar organisationer och ALL deras data för radering, i FK-ordning:
// bytesförfrågningar → pass → användare → organisation.
// Används av DeleteOrganization och av städningen av demo-sandlådor – anroparen kör SaveChangesAsync.
public static class OrganizationRemover
{
    public static async Task RemoveWithAllDataAsync(
        IAppDbContext context,
        IReadOnlyCollection<Guid> organizationIds,
        CancellationToken cancellationToken)
    {
        if (organizationIds.Count == 0)
        {
            return;
        }

        var organizations = await context.Organizations
            .Where(o => organizationIds.Contains(o.Id))
            .ToListAsync(cancellationToken);
        var users = await context.Users
            .Where(u => u.OrganizationId != null && organizationIds.Contains(u.OrganizationId.Value))
            .ToListAsync(cancellationToken);
        var shifts = await context.Shifts
            .Where(s => organizationIds.Contains(s.OrganizationId))
            .ToListAsync(cancellationToken);

        var userIds = users.Select(u => u.Id).ToList();
        var shiftIds = shifts.Select(s => s.Id).ToList();

        // Alla bytesförfrågningar kopplade till organisationernas pass eller användare
        var swapRequests = await context.SwapRequests
            .Where(sr => shiftIds.Contains(sr.ShiftId)
                || userIds.Contains(sr.RequestingUserId)
                || (sr.TargetUserId != null && userIds.Contains(sr.TargetUserId.Value)))
            .ToListAsync(cancellationToken);

        context.SwapRequests.RemoveRange(swapRequests);
        context.Shifts.RemoveRange(shifts);
        context.Users.RemoveRange(users);
        context.Organizations.RemoveRange(organizations);
    }
}
