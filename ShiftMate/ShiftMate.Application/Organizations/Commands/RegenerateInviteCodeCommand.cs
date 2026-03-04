using MediatR;
using Microsoft.EntityFrameworkCore;
using ShiftMate.Application.Interfaces;

namespace ShiftMate.Application.Organizations.Commands
{
    public record RegenerateInviteCodeCommand(Guid OrganizationId) : IRequest<string>;

    public class RegenerateInviteCodeHandler : IRequestHandler<RegenerateInviteCodeCommand, string>
    {
        private readonly IAppDbContext _context;

        public RegenerateInviteCodeHandler(IAppDbContext context)
        {
            _context = context;
        }

        public async Task<string> Handle(RegenerateInviteCodeCommand request, CancellationToken cancellationToken)
        {
            var organization = await _context.Organizations
                .FirstOrDefaultAsync(o => o.Id == request.OrganizationId, cancellationToken);

            if (organization == null)
            {
                throw new InvalidOperationException("Organisationen hittades inte.");
            }

            organization.InviteCode = InviteCodeGenerator.GenerateInviteCode();
            organization.InviteCodeGeneratedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync(cancellationToken);

            return organization.InviteCode;
        }
    }
}
