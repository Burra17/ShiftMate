using MediatR;
using Microsoft.EntityFrameworkCore;
using ShiftMate.Application.DTOs;
using ShiftMate.Application.Interfaces;

namespace ShiftMate.Application.Shifts.Queries.GetMyShifts;

// Query handler för att hämta alla pass som tillhör en specifik användare inom en organisation.
public class GetMyShiftsQueryHandler : IRequestHandler<GetMyShiftsQuery, List<ShiftDto>>
{
    private readonly IAppDbContext _context;

    public GetMyShiftsQueryHandler(IAppDbContext context)
    {
        _context = context;
    }

    public async Task<List<ShiftDto>> Handle(GetMyShiftsQuery request, CancellationToken cancellationToken)
    {
        var shifts = await _context.Shifts
            .AsNoTracking()
            .Where(s => s.UserId == request.UserId && s.OrganizationId == request.OrganizationId)
            .OrderBy(s => s.StartTime)
            .ToListAsync(cancellationToken);

        var shiftDtos = shifts.Select(shift => new ShiftDto
        {
            Id = shift.Id,
            StartTime = shift.StartTime,
            EndTime = shift.EndTime,
            IsUpForSwap = shift.IsUpForSwap,
            UserId = shift.UserId
        }).ToList();

        return shiftDtos;
    }
}
