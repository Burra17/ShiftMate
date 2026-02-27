using Microsoft.EntityFrameworkCore;
using ShiftMate.Domain;

namespace ShiftMate.Application.Interfaces
{
    public interface IAppDbContext
    {
        DbSet<User> Users { get; }
        DbSet<Shift> Shifts { get; }
        DbSet<SwapRequest> SwapRequests { get; }
        DbSet<Organization> Organizations { get; }

        Task<int> SaveChangesAsync(CancellationToken cancellationToken);
    }
}
