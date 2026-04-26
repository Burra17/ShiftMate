using MediatR;
using System.Text.Json.Serialization;

namespace ShiftMate.Application.Users.Commands.ChangePassword;

// Command för att byta lösenord — UserId sätts av controllern via JWT
public record ChangePasswordCommand : IRequest
{
    [JsonIgnore]
    public Guid UserId { get; set; }
    public string CurrentPassword { get; set; } = string.Empty;
    public string NewPassword { get; set; } = string.Empty;
}
