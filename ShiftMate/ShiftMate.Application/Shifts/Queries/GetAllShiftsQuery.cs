using MediatR;
using ShiftMate.Application.DTOs;

namespace ShiftMate.Application.Shifts.Queries
{
    public record GetAllShiftsQuery(Guid OrganizationId, bool OnlyWithUsers = false) : IRequest<List<ShiftDto>>;
}
