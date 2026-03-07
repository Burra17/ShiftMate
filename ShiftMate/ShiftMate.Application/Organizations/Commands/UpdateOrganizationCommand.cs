using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using ShiftMate.Application.Interfaces;

namespace ShiftMate.Application.Organizations.Commands
{
    public record UpdateOrganizationCommand(Guid Id, string Name) : IRequest;

    public class UpdateOrganizationHandler : IRequestHandler<UpdateOrganizationCommand>
    {
        private readonly IAppDbContext _context;
        private readonly IValidator<UpdateOrganizationCommand> _validator;

        public UpdateOrganizationHandler(IAppDbContext context, IValidator<UpdateOrganizationCommand> validator)
        {
            _context = context;
            _validator = validator;
        }

        public async Task Handle(UpdateOrganizationCommand request, CancellationToken cancellationToken)
        {
            // 1. VALIDERING
            var validationResult = await _validator.ValidateAsync(request, cancellationToken);
            if (!validationResult.IsValid)
            {
                throw new ValidationException(validationResult.Errors);
            }

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
