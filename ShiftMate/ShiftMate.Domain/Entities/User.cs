using ShiftMate.Domain.Enums;

namespace ShiftMate.Domain.Entities;

public class User
{
    public Guid Id { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public string? ResetTokenHash { get; set; }
    public DateTime? ResetTokenExpiresAt { get; set; }
    public bool IsEmailVerified { get; set; }
    public string? EmailVerificationTokenHash { get; set; }
    public DateTime? EmailVerificationTokenExpiresAt { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime? DeactivatedAt { get; set; }
    public Role Role { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Foreign Key: Vilken organisation tillhör användaren? (Nullable för SuperAdmin)
    public Guid? OrganizationId { get; set; }
    public Organization? Organization { get; set; }

    // Navigation Properties (Hjälper EF Core att koppla ihop tabeller)
    public ICollection<Shift> Shifts { get; set; } = new List<Shift>();

    // Relationer för bytesförfrågningar
    public ICollection<SwapRequest> SentSwapRequests { get; set; } = new List<SwapRequest>();
    public ICollection<SwapRequest> ReceivedSwapRequests { get; set; } = new List<SwapRequest>();
}
