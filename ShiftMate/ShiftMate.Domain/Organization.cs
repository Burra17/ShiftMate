using System;
using System.Collections.Generic;

namespace ShiftMate.Domain
{
    public class Organization
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation Properties
        public virtual ICollection<User> Users { get; set; } = new List<User>();
        public virtual ICollection<Shift> Shifts { get; set; } = new List<Shift>();
    }
}
