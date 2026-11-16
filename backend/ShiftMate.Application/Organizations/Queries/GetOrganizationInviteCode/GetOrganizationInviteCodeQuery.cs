using MediatR;

namespace ShiftMate.Application.Organizations.Queries.GetOrganizationInviteCode;

// Query för att hämta en organisations invite code och metadata
public record GetOrganizationInviteCodeQuery(Guid OrganizationId) : IRequest<InviteCodeResult>;

// Resultatmodell för invite code, metadata och när den genererades
public record InviteCodeResult(string InviteCode, string OrganizationName, DateTime GeneratedAt);
