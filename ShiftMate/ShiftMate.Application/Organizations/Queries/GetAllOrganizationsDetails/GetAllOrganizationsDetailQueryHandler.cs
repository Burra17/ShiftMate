using MediatR;
using Microsoft.EntityFrameworkCore;
using ShiftMate.Application.DTOs;
using ShiftMate.Application.Interfaces;

namespace ShiftMate.Application.Organizations.Queries.GetAllOrganizationsDetails;

// Query handler för att hämta detaljerad information om alla organisationer, inklusive antal användare i varje organisation.
public class GetAllOrganizationsDetailQueryHandler : IRequestHandler<GetAllOrganizationsDetailQuery, List<OrganizationDetailDto>>
{
    private readonly IAppDbContext _context;

    public GetAllOrganizationsDetailQueryHandler(IAppDbContext context)
    {
        _context = context;
    }

    public async Task<List<OrganizationDetailDto>> Handle(GetAllOrganizationsDetailQuery request, CancellationToken cancellationToken)
    {
        return await _context.Organizations
            .AsNoTracking()
            .OrderBy(o => o.Name)
            .Select(o => new OrganizationDetailDto
            {
                Id = o.Id,
                Name = o.Name,
                InviteCode = o.InviteCode,
                CreatedAt = o.CreatedAt,
                UserCount = o.Users.Count
            })
            .ToListAsync(cancellationToken);
    }
}
