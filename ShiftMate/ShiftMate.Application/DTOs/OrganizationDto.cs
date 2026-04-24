namespace ShiftMate.Application.DTOs;

// Enkel DTO för att visa grundläggande information om en organisation.
public record OrganizationDto
{
    public Guid Id { get; init; }
    public string Name { get; init; } = string.Empty;
}
