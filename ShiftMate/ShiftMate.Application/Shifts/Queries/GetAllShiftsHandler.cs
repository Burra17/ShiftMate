using MediatR;
using Microsoft.EntityFrameworkCore;
using ShiftMate.Application.DTOs;
using ShiftMate.Application.Interfaces;

namespace ShiftMate.Application.Shifts.Queries
{
    // Handläggaren för GetAllShiftsQuery, som hämtar alla pass i systemet.
    public class GetAllShiftsHandler : IRequestHandler<GetAllShiftsQuery, List<ShiftDto>>
    {
        private readonly IAppDbContext _context;

        public GetAllShiftsHandler(IAppDbContext context)
        {
            _context = context;
        }

        public async Task<List<ShiftDto>> Handle(GetAllShiftsQuery request, CancellationToken cancellationToken)
        {
            // Hämta alla pass från databasen, inklusive den användare som äger passet.
            var shifts = await _context.Shifts
                .Include(s => s.User) // Inkluderar User-objektet för att kunna mappa det till DTO:n.
                .OrderBy(s => s.StartTime)
                .ToListAsync(cancellationToken);

            // Mappa de hämtade passen till ShiftDto-objekt.
            var dtos = shifts.Select(s => new ShiftDto
            {
                Id = s.Id,
                StartTime = s.StartTime,
                EndTime = s.EndTime,
                IsUpForSwap = s.IsUpForSwap,
                UserId = s.UserId, // Lägg till UserId för frontend-filtrering/visning.
                // DurationHours räknas ut automatiskt i DTO-klassen.

                // Mappa User-objektet till en UserDto, inklusive namn.
                User = s.User != null ? new UserDto
                {
                    Id = s.User.Id,
                    Email = s.User.Email,
                    FirstName = s.User.FirstName,
                    LastName = s.User.LastName
                } : null

            }).ToList();

            return dtos;
        }
    }
}