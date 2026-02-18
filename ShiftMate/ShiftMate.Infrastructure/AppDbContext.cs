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

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Detta är en standardkonvertering för enum-typer till strängar i databasen.
            modelBuilder.Entity<User>()
                .Property(u => u.Role)
                .HasConversion<string>();

            // Konvertera SwapRequestStatus-enum till sträng i databasen (ingen datamigration behövs)
            modelBuilder.Entity<SwapRequest>()
                .Property(sr => sr.Status)
                .HasConversion<string>();

            // Konfigurera relationen för den användare som SKAPAR en bytesförfrågan.
            modelBuilder.Entity<SwapRequest>()
                .HasOne(s => s.RequestingUser)
                .WithMany(u => u.SentSwapRequests)
                .HasForeignKey(s => s.RequestingUserId)
                .OnDelete(DeleteBehavior.Restrict);

            // Konfigurera relationen för den användare som TAR EMOT en bytesförfrågan (kan vara null).
            modelBuilder.Entity<SwapRequest>()
                .HasOne(s => s.TargetUser)
                .WithMany(u => u.ReceivedSwapRequests)
                .HasForeignKey(s => s.TargetUserId)
                .OnDelete(DeleteBehavior.Restrict);

            // Konfigurera relationen för Shift (passet som erbjuds för byte).
            modelBuilder.Entity<SwapRequest>()
                .HasOne(sr => sr.Shift)
                .WithMany(s => s.SwapRequests)
                .HasForeignKey(sr => sr.ShiftId)
                .OnDelete(DeleteBehavior.Restrict); // Förhindra radering av ett pass om det är del av en bytesförfrågan.

            // Konfigurera relationen för TargetShift (passet som efterfrågas för byte).
            modelBuilder.Entity<SwapRequest>()
                .HasOne(sr => sr.TargetShift)
                .WithMany() // Ingen inverse navigation i Shift-entiteten för TargetShift.
                .HasForeignKey(sr => sr.TargetShiftId)
                .IsRequired(false) // TargetShiftId är nullable.
                .OnDelete(DeleteBehavior.Restrict); // Förhindra radering av ett pass om det är mål i en bytesförfrågan.
        }
    }
}
