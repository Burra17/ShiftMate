using FluentValidation;
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
        private readonly IValidator<CreateOrganizationCommand> _validator;

        public CreateOrganizationHandler(IAppDbContext context, IValidator<CreateOrganizationCommand> validator)
        {
            _context = context;
            _validator = validator;
        }

        public async Task<Guid> Handle(CreateOrganizationCommand request, CancellationToken cancellationToken)
        {
            // 1. VALIDERING
            var validationResult = await _validator.ValidateAsync(request, cancellationToken);
            if (!validationResult.IsValid)
            {
                throw new ValidationException(validationResult.Errors);
            }

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
                InviteCode = InviteCodeGenerator.GenerateInviteCode(),
                InviteCodeGeneratedAt = DateTime.UtcNow,
                CreatedAt = DateTime.UtcNow
            };

            _context.Organizations.Add(organization);
            await _context.SaveChangesAsync(cancellationToken);

            return organization.Id;
        }
    }
}
