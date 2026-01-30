using MediatR;
using Microsoft.EntityFrameworkCore;
using ShiftMate.Application.Interfaces;
using ShiftMate.Domain;

namespace ShiftMate.Application.SwapRequests.Queries
{
    // 1. Request: Vi skickar inget in, vi vill bara ha listan
    public record GetAvailableSwapsQuery : IRequest<List<SwapRequest>>;

    // 2. Handler
    public class GetAvailableSwapsHandler : IRequestHandler<GetAvailableSwapsQuery, List<SwapRequest>>
    {
        private readonly IAppDbContext _context;

        public GetAvailableSwapsHandler(IAppDbContext context)
        {
            _context = context;
        }

        public async Task<List<SwapRequest>> Handle(GetAvailableSwapsQuery request, CancellationToken cancellationToken)
        {
            // Hämta alla förfrågningar som väntar (Pending)
            // Och inkludera Passet (Shift) och Vem som frågar (RequestingUser)
            return await _context.SwapRequests
                .Include(sq => sq.Shift)
                .Include(sq => sq.RequestingUser)
                .Where(sq => sq.Status == "Pending")
                .ToListAsync(cancellationToken);
        }
    }
}