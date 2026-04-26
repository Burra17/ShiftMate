using MediatR;
using System.Text.Json.Serialization;

namespace ShiftMate.Application.Users.Commands.UpdateProfile;

// Command för att uppdatera användarprofil — UserId sätts av controllern via JWT
public record UpdateProfileCommand : IRequest
{
    [JsonIgnore]
    public Guid UserId { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
}
