using ShiftMate.Domain;
using ShiftMate.Infrastructure;

namespace ShiftMate.Infrastructure
{
    public static class DbInitializer
    {
        public static void Initialize(AppDbContext context)
        {
            // Se till att databasen finns
            context.Database.EnsureCreated();

            // ==========================================
            // 1. STÄDA BORT GAMLA PASS & FÖRFRÅGNINGAR 🧹
            // ==========================================
            // Vi rensar schemat varje gång vi startar (i dev-läge) så vi har färsk data.
            if (context.SwapRequests.Any())
            {
                context.SwapRequests.RemoveRange(context.SwapRequests);
            }

            if (context.Shifts.Any())
            {
                context.Shifts.RemoveRange(context.Shifts);
            }

            // Spara rensningen innan vi lägger till nytt
            context.SaveChanges();

            // ==========================================
            // 2. SKAPA ELLER UPPDATERA ANVÄNDARE 👥
            // ==========================================

            // --- USER 1: ANDRÉ (Du / Live-användaren) ---
            var realUser = context.Users.FirstOrDefault(u => u.Email == "andre20030417@gmail.com");
            if (realUser == null)
            {
                realUser = new User
                {
                    Id = Guid.NewGuid(),
                    Email = "andre20030417@gmail.com",
                    FirstName = "André",
                    LastName = "Pettersson",
                    Role = Role.Employee, 
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("Andre2003")
                };
                context.Users.Add(realUser);
            }

            // --- USER 2: ERIK (Kollegan) ---
            var erikUser = context.Users.FirstOrDefault(u => u.Email == "andre@shiftmate.com");
            if (erikUser == null)
            {
                erikUser = new User
                {
                    Id = Guid.NewGuid(),
                    Email = "andre@shiftmate.com", // Vi behåller mailen enligt önskemål
                    FirstName = "Erik",
                    LastName = "Exempel",
                    Role = Role.Employee,
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("dummy_hash_123")
                };
                context.Users.Add(erikUser);
            }
            else
            {
                // Uppdatera namnet om han hette "Test" förut
                erikUser.FirstName = "Erik";
                erikUser.LastName = "Exempel";
            }

            // --- USER 3: SARA (Ny kollega) ---
            var saraUser = context.Users.FirstOrDefault(u => u.Email == "sara@shiftmate.com");
            if (saraUser == null)
            {
                saraUser = new User
                {
                    Id = Guid.NewGuid(),
                    Email = "sara@shiftmate.com",
                    FirstName = "Sara",
                    LastName = "Svensson",
                    Role = Role.Employee,
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("Svensson123")
                };
                context.Users.Add(saraUser);
            }

            // --- USER 4: MAHMOUD (Ny kollega) ---
            var mahmoudUser = context.Users.FirstOrDefault(u => u.Email == "mahmoud@shiftmate.com");
            if (mahmoudUser == null)
            {
                mahmoudUser = new User
                {
                    Id = Guid.NewGuid(),
                    Email = "mahmoud@shiftmate.com",
                    FirstName = "Mahmoud",
                    LastName = "Al-Sayed",
                    Role = Role.Employee,
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("Mahmoud123")
                };
                context.Users.Add(mahmoudUser);
            }

            context.SaveChanges(); // Spara alla användare så vi får deras IDn

            // ==========================================
            // 3. SKAPA PASS (Fyller schemat) 📅
            // ==========================================
            var today = DateTime.SpecifyKind(DateTime.UtcNow.Date, DateTimeKind.Utc);
            var shifts = new List<Shift>();

            // --- IDAG (Dag 0) ---
            shifts.Add(new Shift { Id = Guid.NewGuid(), UserId = erikUser.Id, StartTime = today.AddHours(7), EndTime = today.AddHours(16), IsUpForSwap = false });
            shifts.Add(new Shift { Id = Guid.NewGuid(), UserId = saraUser.Id, StartTime = today.AddHours(8), EndTime = today.AddHours(17), IsUpForSwap = false });
            shifts.Add(new Shift { Id = Guid.NewGuid(), UserId = realUser.Id, StartTime = today.AddHours(15), EndTime = today.AddHours(23), IsUpForSwap = false }); // Du stänger

            // --- IMORGON (Dag 1) ---
            shifts.Add(new Shift { Id = Guid.NewGuid(), UserId = mahmoudUser.Id, StartTime = today.AddDays(1).AddHours(7), EndTime = today.AddDays(1).AddHours(15), IsUpForSwap = false });
            shifts.Add(new Shift { Id = Guid.NewGuid(), UserId = realUser.Id, StartTime = today.AddDays(1).AddHours(12), EndTime = today.AddDays(1).AddHours(20), IsUpForSwap = true }); // Du vill byta detta! 🔄
            shifts.Add(new Shift { Id = Guid.NewGuid(), UserId = erikUser.Id, StartTime = today.AddDays(1).AddHours(16), EndTime = today.AddDays(1).AddHours(23).AddMinutes(30), IsUpForSwap = false });

            // --- I ÖVERMORGON (Dag 2) ---
            shifts.Add(new Shift { Id = Guid.NewGuid(), UserId = saraUser.Id, StartTime = today.AddDays(2).AddHours(7), EndTime = today.AddDays(2).AddHours(16), IsUpForSwap = true }); // Sara vill byta 🔄
            shifts.Add(new Shift { Id = Guid.NewGuid(), UserId = mahmoudUser.Id, StartTime = today.AddDays(2).AddHours(10), EndTime = today.AddDays(2).AddHours(19), IsUpForSwap = false });
            shifts.Add(new Shift { Id = Guid.NewGuid(), UserId = null, StartTime = today.AddDays(2).AddHours(17), EndTime = today.AddDays(2).AddHours(22), IsUpForSwap = false }); // ÖPPET PASS (Ingen ägare) 🆓

            // --- DAG 3 ---
            shifts.Add(new Shift { Id = Guid.NewGuid(), UserId = realUser.Id, StartTime = today.AddDays(3).AddHours(8), EndTime = today.AddDays(3).AddHours(17), IsUpForSwap = false });
            shifts.Add(new Shift { Id = Guid.NewGuid(), UserId = erikUser.Id, StartTime = today.AddDays(3).AddHours(10), EndTime = today.AddDays(3).AddHours(15), IsUpForSwap = false }); // Kort pass

            // --- DAG 4 (Helg?) ---
            shifts.Add(new Shift { Id = Guid.NewGuid(), UserId = saraUser.Id, StartTime = today.AddDays(4).AddHours(9), EndTime = today.AddDays(4).AddHours(18), IsUpForSwap = false });
            shifts.Add(new Shift { Id = Guid.NewGuid(), UserId = mahmoudUser.Id, StartTime = today.AddDays(4).AddHours(16), EndTime = today.AddDays(4).AddHours(23), IsUpForSwap = false });
            shifts.Add(new Shift { Id = Guid.NewGuid(), UserId = null, StartTime = today.AddDays(4).AddHours(12), EndTime = today.AddDays(4).AddHours(16), IsUpForSwap = false }); // ÖPPET EXTRA-PASS 🆓

            context.Shifts.AddRange(shifts);
            context.SaveChanges();
        }
    }
}