using MediatR;
using Microsoft.EntityFrameworkCore;
using ShiftMate.Application.Common;
using ShiftMate.Application.DTOs;
using ShiftMate.Application.Interfaces;

namespace ShiftMate.Application.Users.Queries.GetAllUsers;

// Handlern för att hämta alla användare i en organisation. Den returnerar en lista med användarinformation, inklusive deras ID, namn och e-postadress.
public class GetAllUsersQueryHandler : IRequestHandler<GetAllUsersQuery, PagedResult<UserDto>>
{
    private readonly IAppDbContext _context;

    public GetAllUsersQueryHandler(IAppDbContext context)
    {
        _context = context;
    }

    public async Task<PagedResult<UserDto>> Handle(GetAllUsersQuery request, CancellationToken cancellationToken)
    {
        var query = _context.Users
            .AsNoTracking()
            .Include(u => u.Organization)
            .Where(u => u.OrganizationId == request.OrganizationId && u.IsActive);

        var totalCount = await query.CountAsync(cancellationToken);

        query = query.OrderBy(u => u.FirstName).ThenBy(u => u.LastName);

        // Paginering: om Page anges, använd Skip/Take
        var page = request.Page ?? 0;
        var pageSize = request.PageSize ?? 0;

        if (page > 0 && pageSize > 0)
        {
            query = query.Skip((page - 1) * pageSize).Take(pageSize);
        }

        var items = await query
            .Select(u => new UserDto
            {
                Id = u.Id,
                Email = u.Email,
                FirstName = u.FirstName,
                LastName = u.LastName,
                Role = u.Role.ToString(),
                OrganizationId = u.OrganizationId,
                OrganizationName = u.Organization!.Name
            })
            .ToListAsync(cancellationToken);

        return new PagedResult<UserDto>
        {
            Items = items,
            TotalCount = totalCount,
            Page = page > 0 ? page : 1,
            PageSize = pageSize > 0 ? pageSize : totalCount
        };
    }
}
