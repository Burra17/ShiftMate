namespace ShiftMate.Application.DTOs
{
    public class SwapRequestDto
    {
        public Guid Id { get; set; }
        public string Status { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }

        // Vi bakar in de andra DTO:erna här
        public ShiftDto Shift { get; set; }
        public UserDto RequestingUser { get; set; }
    }
}