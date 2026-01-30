using ShiftMate.Domain;

namespace ShiftMate.Infrastructure
{
    public static class DbInitializer
    {
        public static void Initialize(AppDbContext context)
        {
            // 1. Kolla om det redan finns användare. I så fall: gör ingenting.
            if (context.Users.Any())
            {
                return; // Databasen är redan seedad (fylld)
            }

            // 2. Skapa användare
            var adminUser = new User
            {
                Id = Guid.NewGuid(),
                FirstName = "Boss",
                LastName = "Bossman",
                Email = "admin@shiftmate.com",
                Role = "Admin",
                PasswordHash = "dummy_hash_123" // Vi fixar riktig hashning senare
            };

            var employeeUser = new User
            {
                Id = Guid.NewGuid(),
                FirstName = "André",
                LastName = "Pettersson",
                Email = "andre@shiftmate.com",
                Role = "Employee",
                PasswordHash = "dummy_hash_123"
            };

            // 3. Spara användarna först (så vi har deras IDn)
            context.Users.AddRange(adminUser, employeeUser);
            context.SaveChanges();

            // 4. Skapa arbetspass (Shifts) kopplade till André
            var shifts = new List<Shift>
            {
                // Ett pass som var igår (historik)
                new Shift
                {
                    Id = Guid.NewGuid(),
                    UserId = employeeUser.Id,
                    StartTime = DateTime.Now.AddDays(-1).Date.AddHours(8), // Igår 08:00
                    EndTime = DateTime.Now.AddDays(-1).Date.AddHours(17),  // Igår 17:00
                    IsUpForSwap = false
                },
                // Ett pass imorgon (som vi kan jobba på)
                new Shift
                {
                    Id = Guid.NewGuid(),
                    UserId = employeeUser.Id,
                    StartTime = DateTime.Now.AddDays(1).Date.AddHours(12), // Imorgon 12:00
                    EndTime = DateTime.Now.AddDays(1).Date.AddHours(20),   // Imorgon 20:00
                    IsUpForSwap = false
                },
                // Ett pass på fredag (som vi vill byta bort!)
                new Shift
                {
                    Id = Guid.NewGuid(),
                    UserId = employeeUser.Id,
                    StartTime = DateTime.Now.AddDays(3).Date.AddHours(07), // Om 3 dagar 07:00
                    EndTime = DateTime.Now.AddDays(3).Date.AddHours(16),   // Om 3 dagar 16:00
                    IsUpForSwap = true // <--- Denna är öppen för byte!
                }
            };

            context.Shifts.AddRange(shifts);
            context.SaveChanges();
        }
    }
}