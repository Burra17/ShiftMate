using MediatR;
using Microsoft.EntityFrameworkCore;
using ShiftMate.Application.DTOs;
using ShiftMate.Application.Interfaces;
using ShiftMate.Domain.Enums;

namespace ShiftMate.Application.SwapRequests.Queries.GetReceivedSwapRequests;

// Handlern för att hämta alla bytesförfrågningar som den inloggade användaren har mottagit.
// Den returnerar en lista av SwapRequestDto som innehåller relevant information om varje förfrågan, inklusive pass och användare.
public class GetReceivedSwapRequestsQueryHandler : IRequestHandler<GetReceivedSwapRequestsQuery, List<SwapRequestDto>>
{
    private readonly IAppDbContext _context;

    public GetReceivedSwapRequestsQueryHandler(IAppDbContext context)
    {
        _context = context;
    }

    public async Task<List<SwapRequestDto>> Handle(GetReceivedSwapRequestsQuery request, CancellationToken cancellationToken)
    {
        // 1. Hämta alla förfrågningar där den inloggade användaren är målet och statusen är "Pending".
        var swapRequests = await _context.SwapRequests
            .AsNoTracking()
            .Where(sr => sr.TargetUserId == request.CurrentUserId && sr.Status == SwapRequestStatus.Pending)
            .Include(sr => sr.RequestingUser) // Användaren som skickade förfrågan
            .Include(sr => sr.Shift)          // Passet som de erbjuder
            .Include(sr => sr.TargetShift)    // Passet som de vill ha i utbyte
            .OrderByDescending(sr => sr.CreatedAt)
            .ToListAsync(cancellationToken);

        // 2. Mappa resultaten till DTOs (Data Transfer Objects) för att skicka till klienten.
        return swapRequests.Select(sr => new SwapRequestDto
        {
            Id = sr.Id,
            Status = sr.Status.ToString(),
            CreatedAt = sr.CreatedAt,
            RequestingUser = new UserDto
            {
                Id = sr.RequestingUser.Id,
                FirstName = sr.RequestingUser.FirstName,
                LastName = sr.RequestingUser.LastName,
                Email = sr.RequestingUser.Email
            },
            Shift = new ShiftDto
            {
                Id = sr.Shift.Id,
                StartTime = sr.Shift.StartTime,
                EndTime = sr.Shift.EndTime
            },
            // Inkludera TargetShift om det finns (för direktbyten)
            TargetShift = sr.TargetShift != null ? new ShiftDto
            {
                Id = sr.TargetShift.Id,
                StartTime = sr.TargetShift.StartTime,
                EndTime = sr.TargetShift.EndTime
            } : null
        }).ToList();
    }
}