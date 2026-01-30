using MediatR;
using Microsoft.EntityFrameworkCore;
using ShiftMate.Application.DTOs; // <--- Se till att vi hittar DTOs
using ShiftMate.Application.Interfaces;

namespace ShiftMate.Application.Shifts.Queries
{
    // 1. DATA: Vi ber om en lista av DTOs nu, inte Entities!
    public record GetMyShiftsQuery(Guid UserId) : IRequest<List<ShiftDto>>;

    // 2. LOGIK
    public class GetMyShiftsHandler : IRequestHandler<GetMyShiftsQuery, List<ShiftDto>>
    {
        private readonly IAppDbContext _context;

        public GetMyShiftsHandler(IAppDbContext context)
        {
            _context = context;
        }

        public async Task<List<ShiftDto>> Handle(GetMyShiftsQuery request, CancellationToken cancellationToken)
        {
            // Hämta pass från databasen...
            var shifts = await _context.Shifts
                .Where(s => s.UserId == request.UserId)
                .OrderBy(s => s.StartTime)
                .ToListAsync(cancellationToken);

            // ...och packa om dem till DTOs
            // (I större projekt använder man "AutoMapper" för detta, men nu gör vi det för hand)
            var shiftDtos = shifts.Select(shift => new ShiftDto
            {
                Id = shift.Id,
                StartTime = shift.StartTime,
                EndTime = shift.EndTime,
                IsUpForSwap = shift.IsUpForSwap
                // DurationHours räknas ut automatiskt i DTO-klassen!
            }).ToList();

            return shiftDtos;
        }
    }
}