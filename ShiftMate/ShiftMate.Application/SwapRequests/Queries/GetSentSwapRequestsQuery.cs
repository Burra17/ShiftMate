using MediatR;
using Microsoft.EntityFrameworkCore;
using ShiftMate.Application.DTOs;
using ShiftMate.Application.Interfaces;
using ShiftMate.Domain;
using System.Text.Json.Serialization;

namespace ShiftMate.Application.SwapRequests.Queries
{
    // Hämtar bytesförfrågningar som den inloggade användaren har skickat.
    public record GetSentSwapRequestsQuery : IRequest<List<SwapRequestDto>>
    {
        [JsonIgnore]
        public Guid CurrentUserId { get; set; }
    }

    // Handläggare för att hämta skickade bytesförfrågningar.
    public class GetSentSwapRequestsQueryHandler : IRequestHandler<GetSentSwapRequestsQuery, List<SwapRequestDto>>
    {
        private readonly IAppDbContext _context;

        public GetSentSwapRequestsQueryHandler(IAppDbContext context)
        {
            _context = context;
        }

        public async Task<List<SwapRequestDto>> Handle(GetSentSwapRequestsQuery request, CancellationToken cancellationToken)
        {
            // Hämta alla förfrågningar som den inloggade användaren har skickat och som fortfarande är väntande.
            var swapRequests = await _context.SwapRequests
                .AsNoTracking()
                .Where(sr => sr.RequestingUserId == request.CurrentUserId && sr.Status == SwapRequestStatus.Pending)
                .Include(sr => sr.Shift)
                .Include(sr => sr.TargetShift)
                .Include(sr => sr.TargetUser)
                .OrderByDescending(sr => sr.CreatedAt)
                .ToListAsync(cancellationToken);

            // Mappa till DTOs
            return swapRequests.Select(sr => new SwapRequestDto
            {
                Id = sr.Id,
                Status = sr.Status.ToString(),
                CreatedAt = sr.CreatedAt,
                Shift = new ShiftDto
                {
                    Id = sr.Shift.Id,
                    StartTime = sr.Shift.StartTime,
                    EndTime = sr.Shift.EndTime
                },
                TargetShift = sr.TargetShift != null ? new ShiftDto
                {
                    Id = sr.TargetShift.Id,
                    StartTime = sr.TargetShift.StartTime,
                    EndTime = sr.TargetShift.EndTime
                } : null,
                // Målpersonens info mappas till rätt fält (TargetUser)
                TargetUser = sr.TargetUser != null ? new UserDto
                {
                    Id = sr.TargetUser.Id,
                    FirstName = sr.TargetUser.FirstName,
                    LastName = sr.TargetUser.LastName,
                    Email = sr.TargetUser.Email
                } : null
            }).ToList();
        }
    }
}
