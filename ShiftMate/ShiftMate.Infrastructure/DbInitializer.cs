using ShiftMate.Domain;

namespace ShiftMate.Infrastructure
{
    public static class DbInitializer
    {
        public static void Initialize(AppDbContext context)
        {
            context.Database.EnsureCreated();

            // ==========================================
            // 1. STÄDA BORT GAMLA PASS & FÖRFRÅGNINGAR
            // ==========================================
            if (context.SwapRequests.Any())
            {
                context.SwapRequests.RemoveRange(context.SwapRequests);
            }

            if (context.Shifts.Any())
            {
                context.Shifts.RemoveRange(context.Shifts);
            }

            context.SaveChanges();

            // ==========================================
            // 2. SKAPA ORGANISATIONER
            // ==========================================
            var org1 = context.Organizations.FirstOrDefault(o => o.Name == "ShiftMate Demo");
            if (org1 == null)
            {
                org1 = new Organization
                {
                    Id = Guid.NewGuid(),
                    Name = "ShiftMate Demo",
                    CreatedAt = DateTime.UtcNow
                };
                context.Organizations.Add(org1);
            }

            var org2 = context.Organizations.FirstOrDefault(o => o.Name == "Testföretaget AB");
            if (org2 == null)
            {
                org2 = new Organization
                {
                    Id = Guid.NewGuid(),
                    Name = "Testföretaget AB",
                    CreatedAt = DateTime.UtcNow
                };
                context.Organizations.Add(org2);
            }

            context.SaveChanges();

            // ==========================================
            // 2b. SKAPA SUPERADMIN (plattformsnivå, ingen organisation)
            // ==========================================
            var superAdmin = context.Users.FirstOrDefault(u => u.Email == "superadmin@shiftmate.com");
            if (superAdmin == null)
            {
                superAdmin = new User
                {
                    Id = Guid.NewGuid(),
                    Email = "superadmin@shiftmate.com",
                    FirstName = "Super",
                    LastName = "Admin",
                    Role = Role.SuperAdmin,
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("SuperAdmin123"),
                    OrganizationId = null
                };
                context.Users.Add(superAdmin);
                context.SaveChanges();
            }

            // ==========================================
            // 3. SKAPA ELLER UPPDATERA ANVÄNDARE (Org 1)
            // ==========================================

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
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("Andre2003"),
                    OrganizationId = org1.Id
                };
                context.Users.Add(realUser);
            }
            else if (realUser.OrganizationId == Guid.Empty)
            {
                realUser.OrganizationId = org1.Id;
            }

            var erikUser = context.Users.FirstOrDefault(u => u.Email == "andre@shiftmate.com");
            if (erikUser == null)
            {
                erikUser = new User
                {
                    Id = Guid.NewGuid(),
                    Email = "andre@shiftmate.com",
                    FirstName = "Erik",
                    LastName = "Exempel",
                    Role = Role.Employee,
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("dummy_hash_123"),
                    OrganizationId = org1.Id
                };
                context.Users.Add(erikUser);
            }
            else
            {
                erikUser.FirstName = "Erik";
                erikUser.LastName = "Exempel";
                if (erikUser.OrganizationId == Guid.Empty) erikUser.OrganizationId = org1.Id;
            }

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
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("Svensson123"),
                    OrganizationId = org1.Id
                };
                context.Users.Add(saraUser);
            }
            else if (saraUser.OrganizationId == Guid.Empty)
            {
                saraUser.OrganizationId = org1.Id;
            }

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
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("Mahmoud123"),
                    OrganizationId = org1.Id
                };
                context.Users.Add(mahmoudUser);
            }
            else if (mahmoudUser.OrganizationId == Guid.Empty)
            {
                mahmoudUser.OrganizationId = org1.Id;
            }

            // --- Org 2: Testanvändare ---
            var testUser = context.Users.FirstOrDefault(u => u.Email == "test@testforetaget.se");
            if (testUser == null)
            {
                testUser = new User
                {
                    Id = Guid.NewGuid(),
                    Email = "test@testforetaget.se",
                    FirstName = "Test",
                    LastName = "Användare",
                    Role = Role.Manager,
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("TestPass123"),
                    OrganizationId = org2.Id
                };
                context.Users.Add(testUser);
            }

            context.SaveChanges();

            // ==========================================
            // 4. SKAPA PASS (Org 1)
            // ==========================================
            var today = DateTime.SpecifyKind(DateTime.UtcNow.Date, DateTimeKind.Utc);
            var shifts = new List<Shift>();

            // --- IDAG (Dag 0) ---
            shifts.Add(new Shift { Id = Guid.NewGuid(), UserId = erikUser.Id, StartTime = today.AddHours(7), EndTime = today.AddHours(16), IsUpForSwap = false, OrganizationId = org1.Id });
            shifts.Add(new Shift { Id = Guid.NewGuid(), UserId = saraUser.Id, StartTime = today.AddHours(8), EndTime = today.AddHours(17), IsUpForSwap = false, OrganizationId = org1.Id });
            shifts.Add(new Shift { Id = Guid.NewGuid(), UserId = realUser.Id, StartTime = today.AddHours(15), EndTime = today.AddHours(23), IsUpForSwap = false, OrganizationId = org1.Id });

            // --- IMORGON (Dag 1) ---
            shifts.Add(new Shift { Id = Guid.NewGuid(), UserId = mahmoudUser.Id, StartTime = today.AddDays(1).AddHours(7), EndTime = today.AddDays(1).AddHours(15), IsUpForSwap = false, OrganizationId = org1.Id });
            shifts.Add(new Shift { Id = Guid.NewGuid(), UserId = realUser.Id, StartTime = today.AddDays(1).AddHours(12), EndTime = today.AddDays(1).AddHours(20), IsUpForSwap = true, OrganizationId = org1.Id });
            shifts.Add(new Shift { Id = Guid.NewGuid(), UserId = erikUser.Id, StartTime = today.AddDays(1).AddHours(16), EndTime = today.AddDays(1).AddHours(23).AddMinutes(30), IsUpForSwap = false, OrganizationId = org1.Id });

            // --- I ÖVERMORGON (Dag 2) ---
            shifts.Add(new Shift { Id = Guid.NewGuid(), UserId = saraUser.Id, StartTime = today.AddDays(2).AddHours(7), EndTime = today.AddDays(2).AddHours(16), IsUpForSwap = true, OrganizationId = org1.Id });
            shifts.Add(new Shift { Id = Guid.NewGuid(), UserId = mahmoudUser.Id, StartTime = today.AddDays(2).AddHours(10), EndTime = today.AddDays(2).AddHours(19), IsUpForSwap = false, OrganizationId = org1.Id });
            shifts.Add(new Shift { Id = Guid.NewGuid(), UserId = null, StartTime = today.AddDays(2).AddHours(17), EndTime = today.AddDays(2).AddHours(22), IsUpForSwap = false, OrganizationId = org1.Id });

            // --- DAG 3 ---
            shifts.Add(new Shift { Id = Guid.NewGuid(), UserId = realUser.Id, StartTime = today.AddDays(3).AddHours(8), EndTime = today.AddDays(3).AddHours(17), IsUpForSwap = false, OrganizationId = org1.Id });
            shifts.Add(new Shift { Id = Guid.NewGuid(), UserId = erikUser.Id, StartTime = today.AddDays(3).AddHours(10), EndTime = today.AddDays(3).AddHours(15), IsUpForSwap = false, OrganizationId = org1.Id });

            // --- DAG 4 ---
            shifts.Add(new Shift { Id = Guid.NewGuid(), UserId = saraUser.Id, StartTime = today.AddDays(4).AddHours(9), EndTime = today.AddDays(4).AddHours(18), IsUpForSwap = false, OrganizationId = org1.Id });
            shifts.Add(new Shift { Id = Guid.NewGuid(), UserId = mahmoudUser.Id, StartTime = today.AddDays(4).AddHours(16), EndTime = today.AddDays(4).AddHours(23), IsUpForSwap = false, OrganizationId = org1.Id });
            shifts.Add(new Shift { Id = Guid.NewGuid(), UserId = null, StartTime = today.AddDays(4).AddHours(12), EndTime = today.AddDays(4).AddHours(16), IsUpForSwap = false, OrganizationId = org1.Id });

            // --- Org 2: Ett testpass ---
            shifts.Add(new Shift { Id = Guid.NewGuid(), UserId = testUser.Id, StartTime = today.AddHours(9), EndTime = today.AddHours(17), IsUpForSwap = false, OrganizationId = org2.Id });

            context.Shifts.AddRange(shifts);
            context.SaveChanges();
        }
    }
}
