using MediatR;
using Microsoft.EntityFrameworkCore;
using ShiftMate.Application.DTOs;
using ShiftMate.Application.Interfaces;

namespace ShiftMate.Application.Shifts.Queries
{
    public class GetAllShiftsHandler : IRequestHandler<GetAllShiftsQuery, List<ShiftDto>>
    {
        private readonly IAppDbContext _context;

        public GetAllShiftsHandler(IAppDbContext context)
        {
            _context = context;
        }

        public async Task<List<ShiftDto>> Handle(GetAllShiftsQuery request, CancellationToken cancellationToken)
        {
            // 1. Hämta alla pass och inkludera användardata
            var shifts = await _context.Shifts
                .Include(s => s.User) // Viktigt: Hämtar User-tabellen
                .OrderBy(s => s.StartTime)
                .ToListAsync(cancellationToken);

            // 2. Mappa till DTOer
            var dtos = shifts.Select(s => new ShiftDto
            {
                Id = s.Id,
                StartTime = s.StartTime,
                EndTime = s.EndTime,
                IsUpForSwap = s.IsUpForSwap,
                // DurationHours räknas ut automatiskt i DTO:n

                // 3. Mappa User-objektet (Nu med namn!)
                User = s.User != null ? new UserDto
                {
                    Id = s.User.Id,
                    Email = s.User.Email,
                    FirstName = s.User.FirstName, // <--- NYTT: Förnamn
                    LastName = s.User.LastName    // <--- NYTT: Efternamn
                } : null

            }).ToList();

            return dtos;
        }
    }
}