using MediatR;
using ShiftMate.Application.DTOs;

namespace ShiftMate.Application.Shifts.Queries
{
    // En enkel "beställning" av en lista med ShiftDto från databasen. Vi kan lägga till en parameter för att filtrera på pass som bara har användare, om det skulle behövas i framtiden.
    public record GetAllShiftsQuery(bool OnlyWithUsers = false) : IRequest<List<ShiftDto>>;
}