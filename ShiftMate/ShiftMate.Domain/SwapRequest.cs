using System;

namespace ShiftMate.Domain
{
    public class SwapRequest
    {
        public Guid Id { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public string Status { get; set; } = "Pending"; // Pending, Approved, Rejected

        // Foreign Key: Vilket pass gäller det?
        public Guid ShiftId { get; set; }
        public virtual Shift Shift { get; set; } = null!;

        // Foreign Key: Vem frågar?
        public Guid RequestingUserId { get; set; }
        public virtual User RequestingUser { get; set; } = null!;

        // Foreign Key: Vem får frågan? (Kan vara null om frågan är öppen)
        public Guid? TargetUserId { get; set; }
        public virtual User? TargetUser { get; set; }

        // Foreign Key: Gäller bytet ett specifikt pass? (Används för direktbyten)
        public Guid? TargetShiftId { get; set; }
        public virtual Shift? TargetShift { get; set; }
    }
}