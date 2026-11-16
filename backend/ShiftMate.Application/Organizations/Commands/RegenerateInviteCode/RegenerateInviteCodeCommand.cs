using MediatR;

namespace ShiftMate.Application.Organizations.Commands.RegenerateInviteCode;

// Command för att regenerera en organisations invite code.
public record RegenerateInviteCodeCommand(Guid OrganizationId) : IRequest<string>;
