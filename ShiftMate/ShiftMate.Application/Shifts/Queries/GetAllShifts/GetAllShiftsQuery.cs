using MediatR;
using ShiftMate.Application.Common;
using ShiftMate.Application.DTOs;

namespace ShiftMate.Application.Shifts.Queries.GetAllShifts;

// Query för att hämta alla pass i en organisation, med valfri paginering och möjlighet att filtrera på pass som har användare kopplade.
public record GetAllShiftsQuery(
    Guid OrganizationId,
    bool OnlyWithUsers = false,
    int? Page = null,
    int? PageSize = null) : IRequest<PagedResult<ShiftDto>>;
