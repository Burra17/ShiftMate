using MediatR;
using Microsoft.EntityFrameworkCore;
using ShiftMate.Application.DTOs; // <--- VIKTIG: Vi ska använda DTO
using ShiftMate.Application.Interfaces;
using ShiftMate.Domain;

namespace ShiftMate.Application.Users.Queries
{
    // VIKTIGT: Returnera List<UserDto>, INTE List<User>
    public record GetAllUsersQuery : IRequest<List<UserDto>>;

    public class GetAllUsersHandler : IRequestHandler<GetAllUsersQuery, List<UserDto>>
    {
        private readonly IAppDbContext _context;

        public GetAllUsersHandler(IAppDbContext context)
        {
            _context = context;
        }

        public async Task<List<UserDto>> Handle(GetAllUsersQuery request, CancellationToken cancellationToken)
        {
            // Vi använder .Select() för att plocka ut BARA det vi vill visa.
            // På så sätt skickas inte PasswordHash med, och vi undviker kraschar.
            return await _context.Users
                .Select(u => new UserDto
                {
                    Id = u.Id,
                    Email = u.Email,
                    FirstName = u.FirstName,
                    LastName = u.LastName
                })
                .ToListAsync(cancellationToken);
        }
    }
}