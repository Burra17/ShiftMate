namespace ShiftMate.Domain.Entities;

public class Shift
{
    public Guid Id { get; set; }
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    public bool IsUpForSwap { get; set; }

    // Foreign Key: Vem äger passet?
    public Guid? UserId { get; set; }
    public User? User { get; set; } = null;

    // Foreign Key: Vilken organisation tillhör passet?
    public Guid OrganizationId { get; set; }
    public Organization Organization { get; set; } = null!;

    public ICollection<SwapRequest> SwapRequests { get; set; } = new List<SwapRequest>();
}
