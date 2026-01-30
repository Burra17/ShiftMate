namespace ShiftMate.Application.DTOs
{
    // Detta är ansiktet utåt för ett pass.
    public class ShiftDto
    {
        public Guid Id { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }

        // Vi kan lägga till beräknade fält som frontend älskar!
        // T.ex. hur många timmar passet är.
        public double DurationHours => (EndTime - StartTime).TotalHours;

        public bool IsUpForSwap { get; set; }
    }
}