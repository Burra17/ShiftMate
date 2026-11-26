using ShiftMate.Application.Organizations;
using ShiftMate.Domain.Entities;
using ShiftMate.Domain.Enums;

namespace ShiftMate.Application.Demo;

// Allt som en ny demo-sandlåda innehåller. Visitor är användaren som besökaren loggas in som.
public record DemoSandbox(
    Organization Organization,
    List<User> Users,
    List<Shift> Shifts,
    List<SwapRequest> SwapRequests,
    User Visitor);

// Bygger innehållet i en demo-sandlåda: en organisation med en chef och fyra anställda,
// pass 14 dagar framåt (en del lediga, en del utlagda för byte) och en inkommande
// bytesförfrågan till besökaren – annars går det inte att visa byten när man är ensam.
public static class DemoSandboxBuilder
{
    private const int Days = 14;

    public static DemoSandbox Build(Role visitorRole, string passwordHash, DateTime utcNow)
    {
        // Kort id gör namn och e-post unika mellan sandlådor (unika index i databasen)
        var shortId = Guid.NewGuid().ToString("N")[..6];

        var organization = new Organization
        {
            Id = Guid.NewGuid(),
            Name = $"Demo · {shortId}",
            InviteCode = InviteCodeGenerator.GenerateInviteCode(),
            InviteCodeGeneratedAt = utcNow,
            CreatedAt = utcNow,
            IsDemo = true
        };

        User CreateUser(string firstName, string lastName, Role role) => new()
        {
            Id = Guid.NewGuid(),
            FirstName = firstName,
            LastName = lastName,
            Email = $"{firstName.ToLowerInvariant()}.{shortId}@{DemoConstants.EmailDomain}",
            PasswordHash = passwordHash,
            Role = role,
            IsEmailVerified = true,
            IsActive = true,
            CreatedAt = utcNow,
            OrganizationId = organization.Id,
            Organization = organization
        };

        var manager = CreateUser("Maja", "Lindqvist", Role.Manager);
        var erik = CreateUser("Erik", "Johansson", Role.Employee);
        var sara = CreateUser("Sara", "Nilsson", Role.Employee);
        var ali = CreateUser("Ali", "Hassan", Role.Employee);
        var linnea = CreateUser("Linnea", "Berg", Role.Employee);
        var users = new List<User> { manager, erik, sara, ali, linnea };
        var visitor = visitorRole == Role.Manager ? manager : erik;

        // Tre pass per dag (morgon, dag, kväll) som roterar mellan alla – även chefen har pass
        var today = DateTime.SpecifyKind(utcNow.Date, DateTimeKind.Utc);
        var shifts = new List<Shift>();
        for (var day = 0; day < Days; day++)
        {
            var date = today.AddDays(day);
            shifts.Add(CreateShift(organization, users[day % users.Count], date.AddHours(7), date.AddHours(15)));
            shifts.Add(CreateShift(organization, users[(day + 1) % users.Count], date.AddHours(9), date.AddHours(17)));
            shifts.Add(CreateShift(organization, users[(day + 2) % users.Count], date.AddHours(15), date.AddHours(23)));
        }

        // Var tredje dag är dagpasset ledigt (syns under Lediga Pass)
        for (var day = 1; day < Days; day += 3)
        {
            shifts[(day * 3) + 1].UserId = null;
        }

        var swapRequests = new List<SwapRequest>();

        // Två kollegor har lagt ut ett pass på marknaden (samma form som InitiateSwap skapar)
        foreach (var colleague in new[] { sara, ali })
        {
            var offered = shifts.First(s => s.UserId == colleague.Id && s.StartTime > today.AddDays(2));
            offered.IsUpForSwap = true;
            swapRequests.Add(new SwapRequest
            {
                Id = Guid.NewGuid(),
                ShiftId = offered.Id,
                RequestingUserId = colleague.Id,
                Status = SwapRequestStatus.Pending,
                CreatedAt = utcNow
            });
        }

        // En direkt bytesförfrågan till besökaren (samma form som ProposeDirectSwap skapar)
        var proposer = linnea;
        var proposerShift = shifts.First(s => s.UserId == proposer.Id && !s.IsUpForSwap && s.StartTime > today.AddDays(1));
        var visitorShift = shifts.First(s => s.UserId == visitor.Id && s.StartTime > today.AddDays(1));
        swapRequests.Add(new SwapRequest
        {
            Id = Guid.NewGuid(),
            ShiftId = proposerShift.Id,
            RequestingUserId = proposer.Id,
            TargetUserId = visitor.Id,
            TargetShiftId = visitorShift.Id,
            Status = SwapRequestStatus.Pending,
            CreatedAt = utcNow
        });

        return new DemoSandbox(organization, users, shifts, swapRequests, visitor);
    }

    private static Shift CreateShift(Organization organization, User user, DateTime start, DateTime end) => new()
    {
        Id = Guid.NewGuid(),
        OrganizationId = organization.Id,
        UserId = user.Id,
        StartTime = start,
        EndTime = end
    };
}
