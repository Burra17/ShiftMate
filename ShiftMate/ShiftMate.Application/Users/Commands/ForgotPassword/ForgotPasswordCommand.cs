using MediatR;

namespace ShiftMate.Application.Users.Commands.ForgotPassword;

// Command för att begära lösenordsåterställning via e-post
public record ForgotPasswordCommand : IRequest
{
    public string Email { get; set; } = string.Empty;
}
