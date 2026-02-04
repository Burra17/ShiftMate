using ShiftMate.Domain;
﻿using ShiftMate.Infrastructure;
﻿
﻿namespace ShiftMate.Infrastructure
﻿{
﻿    public static class DbInitializer
﻿    {
﻿        public static void Initialize(AppDbContext context)
﻿        {
﻿            context.Database.EnsureCreated();
﻿
﻿            // 1. Rensa gamla pass, bytesförfrågningar och användare så vi ser nya fräscha datum 🧹
﻿            // Rensa i rätt ordning för att undvika Foreign Key-fel
﻿            if (context.SwapRequests.Any())
﻿            {
﻿                context.SwapRequests.RemoveRange(context.SwapRequests);
﻿                context.SaveChanges(); 
﻿            }
﻿            
﻿            if (context.Shifts.Any())
﻿            {
﻿                context.Shifts.RemoveRange(context.Shifts);
﻿                context.SaveChanges();
﻿            }
﻿            
﻿            // Rensa användare sist, om det behövs (beroende på hur de hanteras)
﻿            // Låter dem vara kvar då vi uppdaterar befintliga istället för att ta bort och lägga till
﻿            // if (context.Users.Any())
﻿            // {
﻿            //     context.Users.RemoveRange(context.Users);
﻿            //     context.SaveChanges();
﻿            // }
﻿            
﻿            // 2. SKAPA ELLER UPPDATERA ANVÄNDARE 👤
﻿            
﻿            // Fixa Admin (Boss)
﻿            var admin = context.Users.FirstOrDefault(u => u.Email == "admin@shiftmate.com");
﻿            if (admin == null)
﻿            {
﻿                admin = new User
﻿                {
﻿                    Id = Guid.NewGuid(),
﻿                    Email = "admin@shiftmate.com",
﻿                    FirstName = "Boss",
﻿                    LastName = "Bossman",
﻿                    Role = Role.Admin,
﻿                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("password")
﻿                };
﻿                context.Users.Add(admin);
﻿            }
﻿            else
﻿            {
﻿                admin.FirstName = "Boss"; 
﻿                admin.LastName = "Bossman";
﻿                admin.PasswordHash = BCrypt.Net.BCrypt.HashPassword("password");
﻿            }
﻿            
﻿            // Fixa André (Employee)
﻿            var andre = context.Users.FirstOrDefault(u => u.Email == "andre@shiftmate.com");
﻿            if (andre == null)
﻿            {
﻿                andre = new User
﻿                {
﻿                    Id = Guid.NewGuid(),
﻿                    Email = "andre@shiftmate.com",
﻿                    FirstName = "André",
﻿                    LastName = "Pettersson",
﻿                    Role = Role.Employee,
﻿                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("dummy_hash_123")
﻿                };
﻿                context.Users.Add(andre);
﻿            }
﻿            else
﻿            {
﻿                andre.FirstName = "André"; 
﻿                andre.LastName = "Pettersson";
﻿                andre.PasswordHash = BCrypt.Net.BCrypt.HashPassword("dummy_hash_123");
﻿            }
﻿            
﻿            context.SaveChanges();
﻿
﻿            // 3. SKAPA EN REJÄL VECKA MED PASS 📅
﻿            var today = DateTime.SpecifyKind(DateTime.UtcNow.Date, DateTimeKind.Utc);
﻿
﻿            var shifts = new List<Shift>
﻿            {
﻿                // --- IGÅR (Historik) ---
﻿                new Shift
﻿                {
﻿                    Id = Guid.NewGuid(), UserId = andre.Id,
﻿                    StartTime = today.AddDays(-1).AddHours(8),
﻿                    EndTime = today.AddDays(-1).AddHours(16),
﻿                    IsUpForSwap = false
﻿                },
﻿
﻿                // --- IDAG ---
﻿                // André kör morgon
﻿                new Shift
﻿                {
﻿                    Id = Guid.NewGuid(), UserId = andre.Id,
﻿                    StartTime = today.AddHours(7),
﻿                    EndTime = today.AddHours(15),
﻿                    IsUpForSwap = false
﻿                },
﻿
﻿                // --- IMORGON (Båda jobbar!) ---
﻿                // Boss kör "kontorstid"
﻿                new Shift
﻿                {
﻿                    Id = Guid.NewGuid(), UserId = admin.Id,
﻿                    StartTime = today.AddDays(1).AddHours(8),
﻿                    EndTime = today.AddDays(1).AddHours(17),
﻿                    IsUpForSwap = false
﻿                },
﻿                // André kör kvällspass samma dag
﻿                new Shift
﻿                {
﻿                    Id = Guid.NewGuid(), UserId = andre.Id,
﻿                    StartTime = today.AddDays(1).AddHours(15),
﻿                    EndTime = today.AddDays(1).AddHours(23),
﻿                    IsUpForSwap = false
﻿                },
﻿
﻿                // --- I ÖVERMORGON (Byte) ---
﻿                // André vill byta bort sitt pass
﻿                new Shift
﻿                {
﻿                    Id = Guid.NewGuid(), UserId = andre.Id,
﻿                    StartTime = today.AddDays(2).AddHours(7),
﻿                    EndTime = today.AddDays(2).AddHours(16),
﻿                    IsUpForSwap = true // <--- Ute på torget! 🔄
﻿                },
﻿
﻿                // --- OM 3 DAGAR ---
﻿                // Boss jobbar kort dag
﻿                new Shift
﻿                {
﻿                    Id = Guid.NewGuid(), UserId = admin.Id,
﻿                    StartTime = today.AddDays(3).AddHours(10),
﻿                    EndTime = today.AddDays(3).AddHours(14),
﻿                    IsUpForSwap = false
﻿                },
﻿
﻿                // --- OM 4 DAGAR (Helg?) ---
﻿                // André kör stängning
﻿                new Shift
﻿                {
﻿                    Id = Guid.NewGuid(), UserId = andre.Id,
﻿                    StartTime = today.AddDays(4).AddHours(16),
﻿                    EndTime = today.AddDays(4).AddHours(23).AddMinutes(30),
﻿                    IsUpForSwap = false
﻿                }
﻿            };
﻿
﻿            context.Shifts.AddRange(shifts);
﻿            context.SaveChanges();
﻿        }
﻿    }
﻿}
﻿