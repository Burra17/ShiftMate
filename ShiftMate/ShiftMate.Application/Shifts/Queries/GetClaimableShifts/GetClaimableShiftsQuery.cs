using MediatR;
using ShiftMate.Application.DTOs;

namespace ShiftMate.Application.Shifts.Queries.GetClaimableShifts;

// Query för att hämta alla pass i en organisation som är tillgängliga att ta över (dvs. inte är tagna av någon användare).
public record GetClaimableShiftsQuery(Guid OrganizationId) : IRequest<List<ShiftDto>>;
