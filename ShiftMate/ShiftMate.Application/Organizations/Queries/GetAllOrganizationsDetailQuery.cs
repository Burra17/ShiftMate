using MediatR;
using Microsoft.EntityFrameworkCore;
using ShiftMate.Application.DTOs;
using ShiftMate.Application.Interfaces;

namespace ShiftMate.Application.Organizations.Queries
{
    public record GetAllOrganizationsDetailQuery : IRequest<List<OrganizationDetailDto>>;

    public class GetAllOrganizationsDetailHandler : IRequestHandler<GetAllOrganizationsDetailQuery, List<OrganizationDetailDto>>
    {
        private readonly IAppDbContext _context;

        public GetAllOrganizationsDetailHandler(IAppDbContext context)
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
                    CreatedAt = o.CreatedAt,
                    UserCount = o.Users.Count
                })
                .ToListAsync(cancellationToken);
        }
    }
}
