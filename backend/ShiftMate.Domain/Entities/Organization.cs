namespace ShiftMate.Domain.Entities;

public class Organization
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string InviteCode { get; set; } = string.Empty;
    public DateTime InviteCodeGeneratedAt { get; set; } = DateTime.UtcNow;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Demo-sandlåda som skapats av en besökare – raderas automatiskt efter en tid.
    // Städningen får ALDRIG röra organisationer där detta är false.
    public bool IsDemo { get; set; }

    // Navigation Properties
    public ICollection<User> Users { get; set; } = new List<User>();
    public ICollection<Shift> Shifts { get; set; } = new List<Shift>();
}
