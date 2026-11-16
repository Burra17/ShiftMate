namespace ShiftMate.Application.DTOs;

// DTO för att visa information om en användare, inklusive deras roll och organisationsinformation.
public record UserDto
{
    public Guid Id { get; init; }
    public string FirstName { get; init; } = string.Empty;
    public string LastName { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
    public string Role { get; init; } = string.Empty;
    public Guid? OrganizationId { get; init; }
    public string OrganizationName { get; init; } = string.Empty;
}
