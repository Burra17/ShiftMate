namespace ShiftMate.Application.DTOs
{
    public class ShiftDto
    {
        public Guid Id { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }

        // Din snygga beräkning (rör ej)
        public double DurationHours => (EndTime - StartTime).TotalHours;

        public bool IsUpForSwap { get; set; }

        // NYTT: Nu kan frontend se vem som jobbar! 👇
        public UserDto? User { get; set; }
    }
}