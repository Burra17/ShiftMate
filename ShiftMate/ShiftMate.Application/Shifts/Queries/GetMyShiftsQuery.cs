using MediatR;
using Microsoft.EntityFrameworkCore;
using ShiftMate.Application.DTOs;
using ShiftMate.Application.Interfaces;

namespace ShiftMate.Application.Shifts.Queries
{
    public record GetMyShiftsQuery(Guid UserId, Guid OrganizationId) : IRequest<List<ShiftDto>>;

    public class GetMyShiftsHandler : IRequestHandler<GetMyShiftsQuery, List<ShiftDto>>
    {
        private readonly IAppDbContext _context;

        public GetMyShiftsHandler(IAppDbContext context)
        {
            _context = context;
        }

        public async Task<List<ShiftDto>> Handle(GetMyShiftsQuery request, CancellationToken cancellationToken)
        {
            var shifts = await _context.Shifts
                .AsNoTracking()
                .Where(s => s.UserId == request.UserId && s.OrganizationId == request.OrganizationId)
                .OrderBy(s => s.StartTime)
                .ToListAsync(cancellationToken);

            var shiftDtos = shifts.Select(shift => new ShiftDto
            {
                Id = shift.Id,
                StartTime = shift.StartTime,
                EndTime = shift.EndTime,
                IsUpForSwap = shift.IsUpForSwap,
                UserId = shift.UserId
            }).ToList();

            return shiftDtos;
        }
    }
}
