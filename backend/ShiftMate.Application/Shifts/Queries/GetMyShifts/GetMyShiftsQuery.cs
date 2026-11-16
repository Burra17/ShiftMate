using MediatR;
using ShiftMate.Application.DTOs;

namespace ShiftMate.Application.Shifts.Queries.GetMyShifts;

// Query för att hämta alla pass som en användare är schemalagd på i en specifik organisation.
public record GetMyShiftsQuery(Guid UserId, Guid OrganizationId) : IRequest<List<ShiftDto>>;
