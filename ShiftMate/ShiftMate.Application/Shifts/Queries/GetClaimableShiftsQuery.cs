using MediatR;
using Microsoft.EntityFrameworkCore;
using ShiftMate.Application.DTOs;
using ShiftMate.Application.Interfaces;

namespace ShiftMate.Application.Shifts.Queries
{
    public record GetClaimableShiftsQuery(Guid OrganizationId) : IRequest<List<ShiftDto>>;

    public class GetClaimableShiftsHandler : IRequestHandler<GetClaimableShiftsQuery, List<ShiftDto>>
    {
        private readonly IAppDbContext _context;

        public GetClaimableShiftsHandler(IAppDbContext context)
        {
            _context = context;
        }

        public async Task<List<ShiftDto>> Handle(GetClaimableShiftsQuery request, CancellationToken cancellationToken)
        {
            var shifts = await _context.Shifts
                .AsNoTracking()
                .Include(s => s.User)
                .Where(s => s.OrganizationId == request.OrganizationId)
                .Where(s => s.IsUpForSwap == true || s.UserId == null)
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
