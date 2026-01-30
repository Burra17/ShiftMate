using MediatR;
using Microsoft.EntityFrameworkCore;
using ShiftMate.Application.DTOs; // <--- Använd våra nya DTOs
using ShiftMate.Application.Interfaces;

namespace ShiftMate.Application.SwapRequests.Queries
{
    // 1. Request: Nu returnerar vi en lista av DTOs!
    public record GetAvailableSwapsQuery : IRequest<List<SwapRequestDto>>;

    // 2. Handler
    public class GetAvailableSwapsHandler : IRequestHandler<GetAvailableSwapsQuery, List<SwapRequestDto>>
    {
        private readonly IAppDbContext _context;

        public GetAvailableSwapsHandler(IAppDbContext context)
        {
            _context = context;
        }

        public async Task<List<SwapRequestDto>> Handle(GetAvailableSwapsQuery request, CancellationToken cancellationToken)
        {
            // Här gör vi "mappningen" (översättningen) från Databas -> DTO
            return await _context.SwapRequests
                .Include(sq => sq.Shift)
                .Include(sq => sq.RequestingUser)
                .Where(sq => sq.Status == "Pending")
                .Select(sq => new SwapRequestDto // <--- VIKTIGT: Här väljer vi vad som ska visas
                {
                    Id = sq.Id,
                    Status = sq.Status,
                    CreatedAt = sq.CreatedAt,

                    Shift = new ShiftDto
                    {
                        Id = sq.Shift.Id,
                        StartTime = sq.Shift.StartTime,
                        EndTime = sq.Shift.EndTime
                    },

                    RequestingUser = new UserDto
                    {
                        Id = sq.RequestingUser.Id,
                        FirstName = sq.RequestingUser.FirstName,
                        LastName = sq.RequestingUser.LastName,
                        Email = sq.RequestingUser.Email
                    }
                })
                .ToListAsync(cancellationToken);
        }
    }
}