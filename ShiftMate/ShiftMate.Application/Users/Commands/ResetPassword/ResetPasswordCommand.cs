using MediatR;

namespace ShiftMate.Application.Users.Commands.ResetPassword;

// Command för att återställa lösenord med token från e-post
public record ResetPasswordCommand : IRequest
{
    public string Token { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string NewPassword { get; set; } = string.Empty;
}
