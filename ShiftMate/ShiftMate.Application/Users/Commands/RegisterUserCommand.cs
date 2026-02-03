using MediatR;
using ShiftMate.Application.DTOs;

namespace ShiftMate.Application.Users.Commands
{
    // Denna record representerar kommandot för att registrera en ny användare.
    // Den innehåller all data som behövs för att skapa en användare.
    // Vi specificerar att den ska returnera en UserDto efter att ha slutförts.
    public record RegisterUserCommand(
        string FirstName,
        string LastName,
        string Email,
        string Password) : IRequest<UserDto>;
}
