using MediatR;
using ShiftMate.Application.DTOs;

namespace ShiftMate.Application.Shifts.Queries
{
    public record GetAllShiftsQuery(
        Guid OrganizationId,
        bool OnlyWithUsers = false,
        int? Page = null,
        int? PageSize = null) : IRequest<PagedResult<ShiftDto>>;
}
