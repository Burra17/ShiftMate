using Microsoft.EntityFrameworkCore;
using ShiftMate.Domain.Entities;

namespace ShiftMate.Application.Interfaces;

// Interface för applikationens databas kontext, som definierar DbSet för varje entitet och en metod för att spara ändringar asynkront.
public interface IAppDbContext
{
    DbSet<User> Users { get; }
    DbSet<Shift> Shifts { get; }
    DbSet<SwapRequest> SwapRequests { get; }
    DbSet<Organization> Organizations { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
