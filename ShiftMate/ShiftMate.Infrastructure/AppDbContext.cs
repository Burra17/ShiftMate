using Microsoft.EntityFrameworkCore;
using ShiftMate.Domain;
using System.Collections.Generic;
using System.Reflection.Emit;

namespace ShiftMate.Infrastructure
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        public DbSet<User> Users { get; set; }
        public DbSet<Shift> Shifts { get; set; }
        public DbSet<SwapRequest> SwapRequests { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Konfigurera relationen för den som SKAPAR förfrågan
            modelBuilder.Entity<SwapRequest>()
                .HasOne(s => s.RequestingUser)
                .WithMany(u => u.SentSwapRequests)
                .HasForeignKey(s => s.RequestingUserId)
                .OnDelete(DeleteBehavior.Restrict);

            // Konfigurera relationen för den som TAR EMOT förfrågan
            modelBuilder.Entity<SwapRequest>()
                .HasOne(s => s.TargetUser)
                .WithMany(u => u.ReceivedSwapRequests)
                .HasForeignKey(s => s.TargetUserId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}