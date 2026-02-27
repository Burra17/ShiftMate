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
            var query = _context.Shifts
                .AsNoTracking()
                .Include(s => s.User)
                .Where(s => s.OrganizationId == request.OrganizationId)
                .AsQueryable();

            if (request.OnlyWithUsers)
            {
                query = query.Where(s => s.UserId != null);
            }

            var shifts = await query
                .OrderBy(s => s.StartTime)
                .ToListAsync(cancellationToken);

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
