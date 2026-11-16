using MediatR;
using ShiftMate.Application.DTOs;

namespace ShiftMate.Application.Users.Commands.RegisterUser;

// Command för att registrera en ny användare.
// Den innehåller alla nödvändiga fält för att skapa en användare och ett invite code för att säkerställa att endast inbjudna kan registrera sig.
// Returnerar en UserDto med den nya användarens information.
public record RegisterUserCommand(
    string FirstName,
    string LastName,
    string Email,
    string Password,
    string InviteCode) : IRequest<UserDto>;
