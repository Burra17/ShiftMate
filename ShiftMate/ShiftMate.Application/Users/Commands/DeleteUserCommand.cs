using MediatR;
using ShiftMate.Application.Interfaces;

namespace ShiftMate.Application.Users.Commands
{
    public record DeleteUserCommand : IRequest<bool>
    {
        public Guid TargetUserId { get; init; }
        public Guid RequestingUserId { get; init; }
    }

    public class DeleteUserHandler : IRequestHandler<DeleteUserCommand, bool>
    {
        private readonly IAppDbContext _context;
        public DeleteUserHandler(IAppDbContext context) { _context = context; }

        public async Task<bool> Handle(DeleteUserCommand request, CancellationToken cancellationToken)
        {
            // Förhindra att manager raderar sig själv
            if (request.TargetUserId == request.RequestingUserId)
                throw new InvalidOperationException("Du kan inte radera ditt eget konto.");

            var user = await _context.Users.FindAsync(new object[] { request.TargetUserId }, cancellationToken);
            if (user == null)
                throw new InvalidOperationException("Användaren hittades inte.");

            _context.Users.Remove(user);
            await _context.SaveChangesAsync(cancellationToken);
            return true;
        }
    }
}
