using MediatR;
using Microsoft.EntityFrameworkCore;
using ShiftMate.Application.DTOs;
using ShiftMate.Application.Interfaces;
using System.Linq;
using System.Collections.Generic;

namespace ShiftMate.Application.Shifts.Queries
{
    // Query för att hämta alla pass som är tillgängliga att "ta" (claim).
    // Detta inkluderar pass som är uppe för byte (IsUpForSwap) samt pass som inte är tilldelade någon användare (UserId = null).
    public record GetClaimableShiftsQuery : IRequest<List<ShiftDto>>;

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
                .Include(s => s.User) // Inkludera User-objektet för att kunna mappa det till DTO:n.
                .Where(s => s.IsUpForSwap == true || s.UserId == null) // Filtrera på lediga pass eller pass uppe för byte
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
