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
            // 1. Skapa en Queryable så vi kan bygga på filter dynamiskt
            var query = _context.Shifts
                .AsNoTracking()
                .Include(s => s.User)
                .AsQueryable();

            // 2. Om OnlyWithUsers är true (t.ex. vid direktbyte), filtrera bort pass där UserId är null
            if (request.OnlyWithUsers)
            {
                query = query.Where(s => s.UserId != null);
            }

            // 3. Hämta datan från databasen
            var shifts = await query
                .OrderBy(s => s.StartTime)
                .ToListAsync(cancellationToken);

            // 4. Mappa de hämtade passen till ShiftDto-objekt.
            var dtos = shifts.Select(s => new ShiftDto
            {
                Id = s.Id,
                StartTime = s.StartTime,
                EndTime = s.EndTime,
                IsUpForSwap = s.IsUpForSwap,
                UserId = s.UserId,

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