using MediatR;
using ShiftMate.Application.DTOs;

namespace ShiftMate.Application.Shifts.Queries
{
    // En enkel "beställning" av en lista med ShiftDto
    public record GetAllShiftsQuery : IRequest<List<ShiftDto>>;
}