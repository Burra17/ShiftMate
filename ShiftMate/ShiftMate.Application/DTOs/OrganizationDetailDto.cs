namespace ShiftMate.Application.DTOs;

// DTO för att visa detaljerad information om en organisation, inklusive antal användare.
public record OrganizationDetailDto
{
    public Guid Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string InviteCode { get; init; } = string.Empty;
    public DateTime CreatedAt { get; init; }
    public int UserCount { get; init; }
}
