using MediatR;

namespace ShiftMate.Application.Users.Commands.DeleteUser;

// Command för att radera en användare.
// TargetUserId är den användare som ska raderas, RequestingUserId är den som gör förfrågan (för validering av behörighet)
// och OrganizationId används för att säkerställa att operationen sker inom rätt organisation.
public record DeleteUserCommand : IRequest<bool>
{
    public Guid TargetUserId { get; init; }
    public Guid RequestingUserId { get; init; }
    public Guid OrganizationId { get; init; }
}
