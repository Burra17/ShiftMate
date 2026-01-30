using MediatR;
using Microsoft.EntityFrameworkCore;
using ShiftMate.Application.DTOs;
using ShiftMate.Application.Interfaces;

namespace ShiftMate.Application.Shifts.Queries
{
    // 1. DATA: Vi behöver veta vems pass vi ska hämta
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
            return await _context.Shifts
                .Where(s => s.UserId == request.UserId) // <--- Filtrera på användarens ID
                .OrderBy(s => s.StartTime)              // Sortera så nästa pass kommer först
                .Select(s => new ShiftDto
                {
                    Id = s.Id,
                    StartTime = s.StartTime,
                    EndTime = s.EndTime
                })
                .ToListAsync(cancellationToken);
        }
    }
}