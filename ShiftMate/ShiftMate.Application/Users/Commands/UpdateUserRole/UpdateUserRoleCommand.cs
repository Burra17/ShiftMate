using MediatR;

namespace ShiftMate.Application.Users.Commands.UpdateUserRole;

// Command för att uppdatera en användares roll.
// Den innehåller TargetUserId, NewRole, RequestingUserId och OrganizationId för att säkerställa att endast behöriga användare kan utföra denna åtgärd.
public record UpdateUserRoleCommand : IRequest<bool>
{
    public Guid TargetUserId { get; init; }
    public string NewRole { get; init; } = string.Empty;
    public Guid RequestingUserId { get; init; }
    public Guid OrganizationId { get; init; }
}
