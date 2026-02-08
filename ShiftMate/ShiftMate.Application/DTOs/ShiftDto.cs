namespace ShiftMate.Application.DTOs
{
    public class ShiftDto
    {
        public Guid Id { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }

        // Beräkning av passets varaktighet i timmar.
        public double DurationHours => (EndTime - StartTime).TotalHours;

        public bool IsUpForSwap { get; set; }

        // Användarens ID som äger passet, viktig för filtrering på frontend.
        public Guid? UserId { get; set; }

        // DTO för användaren som äger passet, för att visa information på frontend.
        public UserDto? User { get; set; }
    }
}