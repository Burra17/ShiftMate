using MediatR;
using Microsoft.EntityFrameworkCore;
using ShiftMate.Application.DTOs;
using ShiftMate.Application.Interfaces;

namespace ShiftMate.Application.Shifts.Queries
{
    // Datamodell för att fråga efter en lista med användarens egna pass.
    public record GetMyShiftsQuery(Guid UserId) : IRequest<List<ShiftDto>>;

    // Handläggaren för GetMyShiftsQuery.
    public class GetMyShiftsHandler : IRequestHandler<GetMyShiftsQuery, List<ShiftDto>>
    {
        private readonly IAppDbContext _context;

        public GetMyShiftsHandler(IAppDbContext context)
        {
            _context = context;
        }

        public async Task<List<ShiftDto>> Handle(GetMyShiftsQuery request, CancellationToken cancellationToken)
        {
            // Hämta pass från databasen som tillhör den angivna användaren, sorterade efter starttid.
            var shifts = await _context.Shifts
                .AsNoTracking()
                .Where(s => s.UserId == request.UserId)
                .OrderBy(s => s.StartTime)
                .ToListAsync(cancellationToken);

            // Mappa om de hämtade passen till ShiftDto-objekt.
            var shiftDtos = shifts.Select(shift => new ShiftDto
            {
                Id = shift.Id,
                StartTime = shift.StartTime,
                EndTime = shift.EndTime,
                IsUpForSwap = shift.IsUpForSwap,
                UserId = shift.UserId // Inkludera UserId för frontend-filtrering/visning.
                // DurationHours räknas ut automatiskt i DTO-klassen.
            }).ToList();

            return shiftDtos;
        }
    }
}