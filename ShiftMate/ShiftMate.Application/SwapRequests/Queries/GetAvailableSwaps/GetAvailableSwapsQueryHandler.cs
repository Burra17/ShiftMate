using MediatR;
using Microsoft.EntityFrameworkCore;
using ShiftMate.Application.DTOs;
using ShiftMate.Application.Interfaces;
using ShiftMate.Domain.Enums;

namespace ShiftMate.Application.SwapRequests.Queries.GetAvailableSwaps;

// Handlern för att hämta alla tillgängliga bytesförfrågningar i en organisation.
// Den returnerar en lista av SwapRequestDto som innehåller relevant information om varje förfrågan, inklusive pass och användare.
public class GetAvailableSwapsQueryHandler : IRequestHandler<GetAvailableSwapsQuery, List<SwapRequestDto>>
{
    private readonly IAppDbContext _context;

    public GetAvailableSwapsQueryHandler(IAppDbContext context)
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
