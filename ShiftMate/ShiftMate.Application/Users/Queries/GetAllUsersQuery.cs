using MediatR;
using Microsoft.EntityFrameworkCore;
using ShiftMate.Application.DTOs;
using ShiftMate.Application.Interfaces;

namespace ShiftMate.Application.Users.Queries
{
    public record GetAllUsersQuery(Guid OrganizationId) : IRequest<List<UserDto>>;

    public class GetAllUsersHandler : IRequestHandler<GetAllUsersQuery, List<UserDto>>
    {
        private readonly IAppDbContext _context;

        public GetAllUsersHandler(IAppDbContext context)
        {
            _context = context;
        }

        public async Task<List<UserDto>> Handle(GetAllUsersQuery request, CancellationToken cancellationToken)
        {
            return await _context.Users
                .AsNoTracking()
                .Include(u => u.Organization)
                .Where(u => u.OrganizationId == request.OrganizationId)
                .Select(u => new UserDto
                {
                    Id = u.Id,
                    Email = u.Email,
                    FirstName = u.FirstName,
                    LastName = u.LastName,
                    Role = u.Role.ToString(),
                    OrganizationId = u.OrganizationId,
                    OrganizationName = u.Organization.Name
                })
                .ToListAsync(cancellationToken);
        }
    }
}
