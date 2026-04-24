using MediatR;
using Microsoft.EntityFrameworkCore;
using ShiftMate.Application.DTOs;
using ShiftMate.Application.Interfaces;

namespace ShiftMate.Application.Organizations.Queries.GetAllOrganizations;

// Query handler för att hämta alla organisationer i systemet, sorterade i alfabetisk ordning.
public class GetAllOrganizationsQueryHandler : IRequestHandler<GetAllOrganizationsQuery, List<OrganizationDto>>
{
    private readonly IAppDbContext _context;

    public GetAllOrganizationsQueryHandler(IAppDbContext context)
    {
        _context = context;
    }

    public async Task<List<OrganizationDto>> Handle(GetAllOrganizationsQuery request, CancellationToken cancellationToken)
    {
        return await _context.Organizations
            .AsNoTracking()
            .OrderBy(o => o.Name)
            .Select(o => new OrganizationDto
            {
                Id = o.Id,
                Name = o.Name
            })
            .ToListAsync(cancellationToken);
    }
}
