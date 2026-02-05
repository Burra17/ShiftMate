using ShiftMate.Domain;
using ShiftMate.Infrastructure;

namespace ShiftMate.Infrastructure
{
    public static class DbInitializer
    {
        public static void Initialize(AppDbContext context)
        {
            context.Database.EnsureCreated();

            // 1. Rensa gamla pass och förfrågningar 🧹
            if (context.SwapRequests.Any())
            {
                context.SwapRequests.RemoveRange(context.SwapRequests);
                context.SaveChanges();
            }

            if (context.Shifts.Any())
            {
                context.Shifts.RemoveRange(context.Shifts);
                context.SaveChanges();
            }

            // 2. SKAPA ELLER UPPDATERA ANVÄNDARE 👤

            // --- USER 1: ADMIN (Boss) ---
            var admin = context.Users.FirstOrDefault(u => u.Email == "admin@shiftmate.com");
            if (admin == null)
            {
                admin = new User
                {
                    Id = Guid.NewGuid(),
                    Email = "admin@shiftmate.com",
                    FirstName = "Boss",
                    LastName = "Bossman",
                    Role = Role.Admin,
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("password")
                };
                context.Users.Add(admin);
            }
            // Inget else behövs här om vi inte vill byta namn på Boss

            // --- USER 2: ERIK EXEMPEL (Tidigare Test-André) ---
            // Vi byter namn på honom så det blir tydligt i schemat!
            var testUser = context.Users.FirstOrDefault(u => u.Email == "andre@shiftmate.com");
            if (testUser == null)
            {
                testUser = new User
                {
                    Id = Guid.NewGuid(),
                    Email = "andre@shiftmate.com",
                    FirstName = "Erik",     // <--- NYTT NAMN
                    LastName = "Exempel",   // <--- NYTT NAMN
                    Role = Role.Employee,
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("dummy_hash_123")
                };
                context.Users.Add(testUser);
            }
            else
            {
                // Om han redan finns, uppdatera namnet till Erik!
                testUser.FirstName = "Erik";
                testUser.LastName = "Exempel";
                testUser.PasswordHash = BCrypt.Net.BCrypt.HashPassword("dummy_hash_123");
            }

            // --- USER 3: DIN RIKTIGA USER (Live) ---
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
            else
            {
                // Se till att du heter André och har rätt lösen
                realUser.FirstName = "André";
                realUser.PasswordHash = BCrypt.Net.BCrypt.HashPassword("Andre2003");
            }

            context.SaveChanges();

            // 3. SKAPA PASS (En mix för Erik, André och Boss) 📅
            var today = DateTime.SpecifyKind(DateTime.UtcNow.Date, DateTimeKind.Utc);

            var shifts = new List<Shift>
            {
                // --- IDAG ---
                // Erik (Kollegan) jobbar förmiddag
                new Shift
                {
                    Id = Guid.NewGuid(), UserId = testUser.Id,
                    StartTime = today.AddHours(7),
                    EndTime = today.AddHours(15),
                    IsUpForSwap = false
                },
                // Du (André) jobbar kväll
                new Shift
                {
                    Id = Guid.NewGuid(), UserId = realUser.Id,
                    StartTime = today.AddHours(15),
                    EndTime = today.AddHours(23),
                    IsUpForSwap = false
                },

                // --- IMORGON ---
                // Boss jobbar dagtid
                new Shift
                {
                    Id = Guid.NewGuid(), UserId = admin.Id,
                    StartTime = today.AddDays(1).AddHours(8),
                    EndTime = today.AddDays(1).AddHours(17),
                    IsUpForSwap = false
                },
                // Du (André) jobbar också, men vill byta bort det?
                new Shift
                {
                    Id = Guid.NewGuid(), UserId = realUser.Id,
                    StartTime = today.AddDays(1).AddHours(12),
                    EndTime = today.AddDays(1).AddHours(20),
                    IsUpForSwap = true // <--- Ute på torget!
                },

                // --- I ÖVERMORGON ---
                // Erik (Kollegan) har ett pass han vill bli av med
                new Shift
                {
                    Id = Guid.NewGuid(), UserId = testUser.Id,
                    StartTime = today.AddDays(2).AddHours(7),
                    EndTime = today.AddDays(2).AddHours(16),
                    IsUpForSwap = true // <--- Testa att ta detta!
                },

                // --- OM 3 DAGAR ---
                // Boss jobbar lunch
                new Shift
                {
                    Id = Guid.NewGuid(), UserId = admin.Id,
                    StartTime = today.AddDays(3).AddHours(10),
                    EndTime = today.AddDays(3).AddHours(14),
                    IsUpForSwap = false
                },
                
                // --- OM 4 DAGAR (Helg) ---
                // Du (André) kör stängning
                new Shift
                {
                    Id = Guid.NewGuid(), UserId = realUser.Id,
                    StartTime = today.AddDays(4).AddHours(16),
                    EndTime = today.AddDays(4).AddHours(23).AddMinutes(30),
                    IsUpForSwap = false
                }
            };

            context.Shifts.AddRange(shifts);
            context.SaveChanges();
        }
    }
}