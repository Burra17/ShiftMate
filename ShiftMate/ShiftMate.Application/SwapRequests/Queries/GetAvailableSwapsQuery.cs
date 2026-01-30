using MediatR;
using Microsoft.EntityFrameworkCore;
using ShiftMate.Application.DTOs;
using ShiftMate.Application.Interfaces;

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
                .Include(sr => sr.Shift)           // Hämta passet
                .Include(sr => sr.RequestingUser)  // <--- VIKTIGT: Hämta användaren också!
                .Where(sr => sr.Status == "Pending")
                .ToListAsync(cancellationToken);

            // Mappa om till DTOs
            var dtos = swaps.Select(sr => new SwapRequestDto
            {
                Id = sr.Id,
                Status = sr.Status,
                CreatedAt = sr.CreatedAt, // <--- Nu tar vi med datumet

                Shift = new ShiftDto
                {
                    Id = sr.Shift.Id,
                    StartTime = sr.Shift.StartTime,
                    EndTime = sr.Shift.EndTime,
                    IsUpForSwap = sr.Shift.IsUpForSwap
                },

                RequestingUser = new UserDto // <--- Nu fyller vi i användaren
                {
                    Id = sr.RequestingUser.Id,
                    Email = sr.RequestingUser.Email
                    // Lägg till Name här om du har lagt till det i UserDto och User-modellen
                }
            }).ToList();

            return dtos;
        }
    }
}