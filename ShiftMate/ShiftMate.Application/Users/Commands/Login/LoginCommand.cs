using MediatR;

namespace ShiftMate.Application.Users.Commands.Login;

// Command för att logga in. Den innehåller email och lösenord, och returnerar en JWT-token vid framgångsrik inloggning.
public record LoginCommand : IRequest<string>
{
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}
