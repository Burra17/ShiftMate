using MediatR;

namespace ShiftMate.Application.Users.Commands.ResendVerification;

// Command för att skicka en ny verifieringsmail. Den innehåller bara email-adressen som behövs för att hitta användaren och skicka mailet.
public record ResendVerificationCommand : IRequest
{
    public string Email { get; set; } = string.Empty;
}
