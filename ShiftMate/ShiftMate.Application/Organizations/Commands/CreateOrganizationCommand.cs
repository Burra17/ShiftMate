using MediatR;
using Microsoft.EntityFrameworkCore;
using ShiftMate.Application.Interfaces;
using ShiftMate.Domain;

namespace ShiftMate.Application.Organizations.Commands
{
    public record CreateOrganizationCommand(string Name) : IRequest<Guid>;

    public class CreateOrganizationHandler : IRequestHandler<CreateOrganizationCommand, Guid>
    {
        private readonly IAppDbContext _context;

        public CreateOrganizationHandler(IAppDbContext context)
        {
            _context = context;
        }

        public async Task<Guid> Handle(CreateOrganizationCommand request, CancellationToken cancellationToken)
        {
            var name = request.Name.Trim();

            var exists = await _context.Organizations
                .AnyAsync(o => o.Name.ToLower() == name.ToLower(), cancellationToken);

            if (exists)
            {
                throw new InvalidOperationException("En organisation med det namnet finns redan.");
            }

            var organization = new Organization
            {
                Id = Guid.NewGuid(),
                Name = name,
                CreatedAt = DateTime.UtcNow
            };

            _context.Organizations.Add(organization);
            await _context.SaveChangesAsync(cancellationToken);

            return organization.Id;
        }
    }
}
