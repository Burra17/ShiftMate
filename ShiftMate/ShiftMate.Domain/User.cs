using System.Collections.Generic; // Behövs för ICollection

namespace ShiftMate.Domain
{
    public class User
    {
        public Guid Id { get; set; }
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string PasswordHash { get; set; } = string.Empty;
        public string Role { get; set; } = "Employee"; // "Admin" eller "Employee"

        // Navigation Properties (Hjälper EF Core att koppla ihop tabeller)
        public virtual ICollection<Shift> Shifts { get; set; } = new List<Shift>();

        // Relationer för bytesförfrågningar
        public virtual ICollection<SwapRequest> SentSwapRequests { get; set; } = new List<SwapRequest>();
        public virtual ICollection<SwapRequest> ReceivedSwapRequests { get; set; } = new List<SwapRequest>();
    }
}