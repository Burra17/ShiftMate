namespace ShiftMate.Application.DTOs;

// DTO för att visa information om ett pass, inklusive varaktighet och ägarinformation.
public record ShiftDto
{
    public Guid Id { get; init; }
    public DateTime StartTime { get; init; }
    public DateTime EndTime { get; init; }

    // Beräkning av passets varaktighet i timmar.
    public double DurationHours => (EndTime - StartTime).TotalHours;

    public bool IsUpForSwap { get; init; }

    // Användarens ID som äger passet, viktig för filtrering på frontend.
    public Guid? UserId { get; init; }

    // DTO för användaren som äger passet, för att visa information på frontend.
    public UserDto? User { get; init; }
}
