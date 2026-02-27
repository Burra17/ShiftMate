using MediatR;
using ShiftMate.Application.DTOs;

namespace ShiftMate.Application.Users.Commands
{
    public record RegisterUserCommand(
        string FirstName,
        string LastName,
        string Email,
        string Password,
        Guid OrganizationId) : IRequest<UserDto>;
}
