using System;
using System.Collections.Generic;

namespace ShiftMate.Domain
{
    public class Shift
    {
        public Guid Id { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public bool IsUpForSwap { get; set; }

        // Foreign Key: Vem äger passet?
        public Guid? UserId { get; set; }
        public virtual User User { get; set; } = null!;

        // Foreign Key: Vilken organisation tillhör passet?
        public Guid OrganizationId { get; set; }
        public virtual Organization Organization { get; set; } = null!;

        public virtual ICollection<SwapRequest> SwapRequests { get; set; } = new List<SwapRequest>();
    }
}
