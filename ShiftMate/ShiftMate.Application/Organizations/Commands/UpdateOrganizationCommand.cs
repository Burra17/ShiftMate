using MediatR;
using Microsoft.EntityFrameworkCore;
using ShiftMate.Application.Interfaces;

namespace ShiftMate.Application.Organizations.Commands
{
    public record UpdateOrganizationCommand(Guid Id, string Name) : IRequest;

    public class UpdateOrganizationHandler : IRequestHandler<UpdateOrganizationCommand>
    {
        private readonly IAppDbContext _context;

        public UpdateOrganizationHandler(IAppDbContext context)
        {
            _context = context;
        }

        public async Task Handle(UpdateOrganizationCommand request, CancellationToken cancellationToken)
        {
            var name = request.Name.Trim();

            var organization = await _context.Organizations
                .FirstOrDefaultAsync(o => o.Id == request.Id, cancellationToken);

            if (organization == null)
            {
                throw new Exception("Organisationen hittades inte.");
            }

            var duplicate = await _context.Organizations
                .AnyAsync(o => o.Id != request.Id && o.Name.ToLower() == name.ToLower(), cancellationToken);

            if (duplicate)
            {
                throw new InvalidOperationException("En organisation med det namnet finns redan.");
            }

            organization.Name = name;
            await _context.SaveChangesAsync(cancellationToken);
        }
    }
}
