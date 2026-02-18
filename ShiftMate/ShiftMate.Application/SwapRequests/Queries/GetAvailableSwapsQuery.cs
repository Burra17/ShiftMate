using MediatR;
using Microsoft.EntityFrameworkCore;
using ShiftMate.Application.DTOs;
using ShiftMate.Application.Interfaces;
using ShiftMate.Domain;

namespace ShiftMate.Application.SwapRequests.Queries
{
    // Returnera en lista av SwapRequestDto
    public record GetAvailableSwapsQuery : IRequest<List<SwapRequestDto>>;

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
                .Include(sr => sr.Shift)           // Hämta passet
                .Include(sr => sr.RequestingUser)  // <--- VIKTIGT: Hämta användaren också!
                .Where(sr => sr.Status == SwapRequestStatus.Pending)
                .ToListAsync(cancellationToken);

            // Mappa om till DTOs
            var dtos = swaps.Select(sr => new SwapRequestDto
            {
                Id = sr.Id,
                Status = sr.Status.ToString(),
                CreatedAt = sr.CreatedAt, // <--- Nu tar vi med datumet

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