using MediatR;
using ShiftMate.Application.Interfaces;
using ShiftMate.Domain;

namespace ShiftMate.Application.Users.Commands
{
    public record UpdateUserRoleCommand : IRequest<bool>
    {
        public Guid TargetUserId { get; init; }
        public string NewRole { get; init; } = string.Empty;
        public Guid RequestingUserId { get; init; }
    }

    public class UpdateUserRoleHandler : IRequestHandler<UpdateUserRoleCommand, bool>
    {
        private readonly IAppDbContext _context;
        public UpdateUserRoleHandler(IAppDbContext context) { _context = context; }

        public async Task<bool> Handle(UpdateUserRoleCommand request, CancellationToken cancellationToken)
        {
            // Förhindra att manager ändrar sin egen roll
            if (request.TargetUserId == request.RequestingUserId)
                throw new InvalidOperationException("Du kan inte ändra din egen roll.");

            if (!Enum.TryParse<Role>(request.NewRole, out var newRole))
                throw new ArgumentException($"Ogiltig roll: {request.NewRole}");

            var user = await _context.Users.FindAsync(new object[] { request.TargetUserId }, cancellationToken);
            if (user == null)
                throw new InvalidOperationException("Användaren hittades inte.");

            user.Role = newRole;
            await _context.SaveChangesAsync(cancellationToken);
            return true;
        }
    }
}
