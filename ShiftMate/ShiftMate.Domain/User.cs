using System.Collections.Generic;

namespace ShiftMate.Domain
{
    public class User
    {
        public Guid Id { get; set; }
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string PasswordHash { get; set; } = string.Empty;
        public string? ResetTokenHash { get; set; }
        public DateTime? ResetTokenExpiresAt { get; set; }
        public Role Role { get; set; }

        // Foreign Key: Vilken organisation tillhör användaren? (Nullable för SuperAdmin)
        public Guid? OrganizationId { get; set; }
        public virtual Organization? Organization { get; set; }

        // Navigation Properties (Hjälper EF Core att koppla ihop tabeller)
        public virtual ICollection<Shift> Shifts { get; set; } = new List<Shift>();

        // Relationer för bytesförfrågningar
        public virtual ICollection<SwapRequest> SentSwapRequests { get; set; } = new List<SwapRequest>();
        public virtual ICollection<SwapRequest> ReceivedSwapRequests { get; set; } = new List<SwapRequest>();
    }

    public enum Role
    {
        Employee,
        Manager,
        SuperAdmin
    }
}
