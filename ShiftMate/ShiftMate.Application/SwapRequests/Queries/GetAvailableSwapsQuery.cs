using MediatR;
using Microsoft.EntityFrameworkCore;
using ShiftMate.Application.DTOs;
using ShiftMate.Application.Interfaces;
using ShiftMate.Domain;

namespace ShiftMate.Application.SwapRequests.Queries
{
    public record GetAvailableSwapsQuery(Guid OrganizationId) : IRequest<List<SwapRequestDto>>;

    public class GetAvailableSwapsHandler : IRequestHandler<GetAvailableSwapsQuery, List<SwapRequestDto>>
    {
        private readonly IAppDbContext _context;

        public GetAvailableSwapsHandler(IAppDbContext context)
        {
            _context = context;
        }

        public async Task<List<SwapRequestDto>> Handle(GetAvailableSwapsQuery request, CancellationToken cancellationToken)
        {
            var swaps = await _context.SwapRequests
                .AsNoTracking()
                .Include(sr => sr.Shift)
                .Include(sr => sr.RequestingUser)
                .Where(sr => sr.Status == SwapRequestStatus.Pending)
                .Where(sr => sr.Shift.OrganizationId == request.OrganizationId)
                .ToListAsync(cancellationToken);

            var dtos = swaps.Select(sr => new SwapRequestDto
            {
                Id = sr.Id,
                Status = sr.Status.ToString(),
                CreatedAt = sr.CreatedAt,
                Shift = new ShiftDto
                {
                    Id = sr.Shift.Id,
                    StartTime = sr.Shift.StartTime,
                    EndTime = sr.Shift.EndTime,
                    IsUpForSwap = sr.Shift.IsUpForSwap
                },
                RequestingUser = new UserDto
                {
                    Id = sr.RequestingUser.Id,
                    FirstName = sr.RequestingUser.FirstName,
                    LastName = sr.RequestingUser.LastName,
                    Email = sr.RequestingUser.Email
                }
            }).ToList();

            return dtos;
        }
    }
}
