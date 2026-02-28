using Microsoft.EntityFrameworkCore;
using ShiftMate.Domain;
using ShiftMate.Application.Interfaces;

namespace ShiftMate.Infrastructure
{
    public class AppDbContext : DbContext, IAppDbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        public DbSet<User> Users { get; set; }
        public DbSet<Shift> Shifts { get; set; }
        public DbSet<SwapRequest> SwapRequests { get; set; }
        public DbSet<Organization> Organizations { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Organization: Unikt namn
            modelBuilder.Entity<Organization>()
                .HasIndex(o => o.Name)
                .IsUnique();

            // User → Organization (valfri för SuperAdmin)
            modelBuilder.Entity<User>()
                .HasOne(u => u.Organization)
                .WithMany(o => o.Users)
                .HasForeignKey(u => u.OrganizationId)
                .IsRequired(false)
                .OnDelete(DeleteBehavior.Restrict);

            // Shift → Organization
            modelBuilder.Entity<Shift>()
                .HasOne(s => s.Organization)
                .WithMany(o => o.Shifts)
                .HasForeignKey(s => s.OrganizationId)
                .OnDelete(DeleteBehavior.Restrict);

            // Enum-konverteringar
            modelBuilder.Entity<User>()
                .Property(u => u.Role)
                .HasConversion<string>();

            modelBuilder.Entity<SwapRequest>()
                .Property(sr => sr.Status)
                .HasConversion<string>();

            // SwapRequest-relationer
            modelBuilder.Entity<SwapRequest>()
                .HasOne(s => s.RequestingUser)
                .WithMany(u => u.SentSwapRequests)
                .HasForeignKey(s => s.RequestingUserId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<SwapRequest>()
                .HasOne(s => s.TargetUser)
                .WithMany(u => u.ReceivedSwapRequests)
                .HasForeignKey(s => s.TargetUserId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<SwapRequest>()
                .HasOne(sr => sr.Shift)
                .WithMany(s => s.SwapRequests)
                .HasForeignKey(sr => sr.ShiftId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<SwapRequest>()
                .HasOne(sr => sr.TargetShift)
                .WithMany()
                .HasForeignKey(sr => sr.TargetShiftId)
                .IsRequired(false)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
