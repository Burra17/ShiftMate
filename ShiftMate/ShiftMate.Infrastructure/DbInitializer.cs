using ShiftMate.Domain;
using ShiftMate.Infrastructure;

namespace ShiftMate.Infrastructure
{
    public static class DbInitializer
    {
        public static void Initialize(AppDbContext context)
        {
            context.Database.EnsureCreated();

            // 1. Rensa gamla pass så vi ser nya fräscha datum 🧹
            // Detta är viktigt för att inte fylla databasen med dubbletter varje gång vi startar om.
            if (context.Shifts.Any())
            {
                context.Shifts.RemoveRange(context.Shifts);
                context.SaveChanges();
            }

            // 2. SKAPA ELLER UPPDATERA ANVÄNDARE 👤

            // Fixa Admin (Boss)
            var admin = context.Users.FirstOrDefault(u => u.Email == "admin@shiftmate.com");
            if (admin == null)
            {
                admin = new User
                {
                    Id = Guid.NewGuid(),
                    Email = "admin@shiftmate.com",
                    FirstName = "Boss",
                    LastName = "Bossman",
                    Role = "Admin",
                    PasswordHash = "dummy_hash_123"
                };
                context.Users.Add(admin);
            }
            else
            {
                admin.FirstName = "Boss"; admin.LastName = "Bossman";
            }

            // Fixa André (Employee)
            var andre = context.Users.FirstOrDefault(u => u.Email == "andre@shiftmate.com");
            if (andre == null)
            {
                andre = new User
                {
                    Id = Guid.NewGuid(),
                    Email = "andre@shiftmate.com",
                    FirstName = "André",
                    LastName = "Pettersson",
                    Role = "Employee",
                    PasswordHash = "dummy_hash_123"
                };
                context.Users.Add(andre);
            }
            else
            {
                andre.FirstName = "André"; andre.LastName = "Pettersson";
            }

            context.SaveChanges();

            // 3. SKAPA EN REJÄL VECKA MED PASS 📅
            var today = DateTime.Now.Date;

            var shifts = new List<Shift>
            {
                // --- IGÅR (Historik) ---
                new Shift
                {
                    Id = Guid.NewGuid(), UserId = andre.Id,
                    StartTime = today.AddDays(-1).AddHours(8),
                    EndTime = today.AddDays(-1).AddHours(16),
                    IsUpForSwap = false
                },

                // --- IDAG ---
                // André kör morgon
                new Shift
                {
                    Id = Guid.NewGuid(), UserId = andre.Id,
                    StartTime = today.AddHours(7),
                    EndTime = today.AddHours(15),
                    IsUpForSwap = false
                },

                // --- IMORGON (Båda jobbar!) ---
                // Boss kör "kontorstid"
                new Shift
                {
                    Id = Guid.NewGuid(), UserId = admin.Id,
                    StartTime = today.AddDays(1).AddHours(8),
                    EndTime = today.AddDays(1).AddHours(17),
                    IsUpForSwap = false
                },
                // André kör kvällspass samma dag
                new Shift
                {
                    Id = Guid.NewGuid(), UserId = andre.Id,
                    StartTime = today.AddDays(1).AddHours(15),
                    EndTime = today.AddDays(1).AddHours(23),
                    IsUpForSwap = false
                },

                // --- I ÖVERMORGON (Byte) ---
                // André vill byta bort sitt pass
                new Shift
                {
                    Id = Guid.NewGuid(), UserId = andre.Id,
                    StartTime = today.AddDays(2).AddHours(7),
                    EndTime = today.AddDays(2).AddHours(16),
                    IsUpForSwap = true // <--- Ute på torget! 🔄
                },

                // --- OM 3 DAGAR ---
                // Boss jobbar kort dag
                new Shift
                {
                    Id = Guid.NewGuid(), UserId = admin.Id,
                    StartTime = today.AddDays(3).AddHours(10),
                    EndTime = today.AddDays(3).AddHours(14),
                    IsUpForSwap = false
                },

                // --- OM 4 DAGAR (Helg?) ---
                // André kör stängning
                new Shift
                {
                    Id = Guid.NewGuid(), UserId = andre.Id,
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