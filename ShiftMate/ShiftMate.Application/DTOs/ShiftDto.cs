namespace ShiftMate.Application.DTOs
{
    public class ShiftDto
    {
        public Guid Id { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
    }
}