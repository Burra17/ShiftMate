namespace ShiftMate.Application.DTOs
{
    public class OrganizationDetailDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public int UserCount { get; set; }
    }
}
