using MediatR;
using Microsoft.EntityFrameworkCore;
using ShiftMate.Application.DTOs;
using ShiftMate.Application.Interfaces;

namespace ShiftMate.Application.Shifts.Queries
{
    public class GetAllShiftsHandler : IRequestHandler<GetAllShiftsQuery, PagedResult<ShiftDto>>
    {
        private readonly IAppDbContext _context;

        public GetAllShiftsHandler(IAppDbContext context)
        {
            _context = context;
        }

        public async Task<PagedResult<ShiftDto>> Handle(GetAllShiftsQuery request, CancellationToken cancellationToken)
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

            var totalCount = await query.CountAsync(cancellationToken);

            query = query.OrderBy(s => s.StartTime);

            // Paginering: om Page anges, använd Skip/Take
            var page = request.Page ?? 0;
            var pageSize = request.PageSize ?? 0;

            if (page > 0 && pageSize > 0)
            {
                query = query.Skip((page - 1) * pageSize).Take(pageSize);
            }

            var shifts = await query.ToListAsync(cancellationToken);

            var items = shifts.Select(s => new ShiftDto
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

            return new PagedResult<ShiftDto>
            {
                Items = items,
                TotalCount = totalCount,
                Page = page > 0 ? page : 1,
                PageSize = pageSize > 0 ? pageSize : totalCount
            };
        }
    }
}
