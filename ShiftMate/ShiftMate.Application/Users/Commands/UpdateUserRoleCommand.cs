using FluentValidation;
using MediatR;
using ShiftMate.Application.Interfaces;
using ShiftMate.Domain.Entities;
using ShiftMate.Domain.Enums;

namespace ShiftMate.Application.Users.Commands
{
    public record UpdateUserRoleCommand : IRequest<bool>
    {
        public Guid TargetUserId { get; init; }
        public string NewRole { get; init; } = string.Empty;
        public Guid RequestingUserId { get; init; }
        public Guid OrganizationId { get; init; }
    }

    public class UpdateUserRoleHandler : IRequestHandler<UpdateUserRoleCommand, bool>
    {
        private readonly IAppDbContext _context;
        private readonly IValidator<UpdateUserRoleCommand> _validator;

        public UpdateUserRoleHandler(IAppDbContext context, IValidator<UpdateUserRoleCommand> validator)
        {
            _context = context;
            _validator = validator;
        }

        public async Task<bool> Handle(UpdateUserRoleCommand request, CancellationToken cancellationToken)
        {
            // 1. VALIDERING
            var validationResult = await _validator.ValidateAsync(request, cancellationToken);
            if (!validationResult.IsValid)
            {
                throw new ValidationException(validationResult.Errors);
            }

            if (request.TargetUserId == request.RequestingUserId)
                throw new InvalidOperationException("Du kan inte ändra din egen roll.");

            if (!Enum.TryParse<Role>(request.NewRole, out var newRole))
                throw new ArgumentException($"Ogiltig roll: {request.NewRole}");

            var user = await _context.Users.FindAsync(new object[] { request.TargetUserId }, cancellationToken);
            if (user == null)
                throw new InvalidOperationException("Användaren hittades inte.");

            // Validera att användaren tillhör samma organisation
            if (user.OrganizationId != request.OrganizationId)
                throw new InvalidOperationException("Användaren tillhör inte din organisation.");

            user.Role = newRole;
            await _context.SaveChangesAsync(cancellationToken);
            return true;
        }
    }
}
