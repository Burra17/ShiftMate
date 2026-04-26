using MediatR;

namespace ShiftMate.Application.Users.Commands.VerifyEmail;

// Command för att verifiera en användares email-adress. Den innehåller token och email som krävs för verifieringen.
public record VerifyEmailCommand : IRequest
{
    public string Token { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
}
