using MediatR;
using Microsoft.EntityFrameworkCore;
using ShiftMate.Application.Interfaces;
using ShiftMate.Domain.Enums;

namespace ShiftMate.Application.Users.Commands
{
    public record DeleteUserCommand : IRequest<bool>
    {
        public Guid TargetUserId { get; init; }
        public Guid RequestingUserId { get; init; }
        public Guid OrganizationId { get; init; }
    }

    public class DeleteUserHandler : IRequestHandler<DeleteUserCommand, bool>
    {
        private readonly IAppDbContext _context;
        public DeleteUserHandler(IAppDbContext context) { _context = context; }

        public async Task<bool> Handle(DeleteUserCommand request, CancellationToken cancellationToken)
        {
            if (request.TargetUserId == request.RequestingUserId)
                throw new InvalidOperationException("Du kan inte inaktivera ditt eget konto.");

            var user = await _context.Users.FindAsync(new object[] { request.TargetUserId }, cancellationToken);
            if (user == null)
                throw new InvalidOperationException("Användaren hittades inte.");

            if (user.OrganizationId != request.OrganizationId)
                throw new InvalidOperationException("Användaren tillhör inte din organisation.");

            if (!user.IsActive)
                throw new InvalidOperationException("Användaren är redan inaktiverad.");

            // Soft delete: markera som inaktiv
            user.IsActive = false;
            user.DeactivatedAt = DateTime.UtcNow;

            // Frigör användarens tilldelade pass (gör dem lediga)
            var userShifts = await _context.Shifts
                .Where(s => s.UserId == request.TargetUserId)
                .ToListAsync(cancellationToken);

            foreach (var shift in userShifts)
            {
                shift.UserId = null;
                shift.IsUpForSwap = false;
            }

            // Avbryt alla väntande bytesförfrågningar som involverar användaren
            var pendingSwaps = await _context.SwapRequests
                .Where(sr => sr.Status == SwapRequestStatus.Pending &&
                    (sr.RequestingUserId == request.TargetUserId || sr.TargetUserId == request.TargetUserId))
                .ToListAsync(cancellationToken);

            foreach (var swap in pendingSwaps)
            {
                swap.Status = SwapRequestStatus.Cancelled;
            }

            await _context.SaveChangesAsync(cancellationToken);
            return true;
        }
    }
}
