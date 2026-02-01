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
            // 1. Hämta alla pass och INKLUDERA User-tabellen
            var shifts = await _context.Shifts
                .Include(s => s.User) // <--- Detta är nyckeln!
                .OrderBy(s => s.StartTime)
                .ToListAsync(cancellationToken);

            // 2. Gör om databas-raderna till våra DTO-paket
            var dtos = shifts.Select(s => new ShiftDto
            {
                Id = s.Id,
                StartTime = s.StartTime,
                EndTime = s.EndTime,
                IsUpForSwap = s.IsUpForSwap,
                // DurationHours räknas ut automatiskt i din DTO

                // Mappa användaren om den finns
                User = s.User != null ? new UserDto
                {
                    Email = s.User.Email
                    // Lägg till fler fält här om UserDto har dem (t.ex. Id)
                } : null
            }).ToList();

            return dtos;
        }
    }
}