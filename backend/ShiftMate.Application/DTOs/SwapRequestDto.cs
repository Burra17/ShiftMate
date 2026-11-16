namespace ShiftMate.Application.DTOs;

// DTO för att visa information om en bytesförfrågan, inklusive detaljer om passet som byts och användarna involverade.
public record SwapRequestDto
{
    public Guid Id { get; init; }
    public string Status { get; init; } = string.Empty;
    public DateTime CreatedAt { get; init; }
    public ShiftDto? Shift { get; init; }
    public UserDto? RequestingUser { get; init; }
    public UserDto? TargetUser { get; init; }
    public ShiftDto? TargetShift { get; init; }
}
