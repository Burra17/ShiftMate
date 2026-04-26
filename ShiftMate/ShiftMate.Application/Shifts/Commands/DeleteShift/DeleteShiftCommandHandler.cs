using MediatR;
using Microsoft.EntityFrameworkCore;
using ShiftMate.Application.Interfaces;
using ShiftMate.Domain.Enums;

namespace ShiftMate.Application.Shifts.Commands.DeleteShift;

// Command handler för att radera ett pass och alla dess relaterade bytesförfrågningar.
public class DeleteShiftCommandHandler : IRequestHandler<DeleteShiftCommand, bool>
{
    private readonly IAppDbContext _context;

    public DeleteShiftCommandHandler(IAppDbContext context)
    {
        _context = context;
    }

    public async Task<bool> Handle(DeleteShiftCommand request, CancellationToken cancellationToken)
    {
        var shift = await _context.Shifts
            .Include(s => s.SwapRequests)
            .FirstOrDefaultAsync(s => s.Id == request.ShiftId, cancellationToken);

        if (shift == null)
        {
            throw new InvalidOperationException("Passet hittades inte.");
        }

        // Validera att passet tillhör samma organisation
        if (shift.OrganizationId != request.OrganizationId)
        {
            throw new InvalidOperationException("Passet tillhör inte din organisation.");
        }

        foreach (var swapRequest in shift.SwapRequests.ToList())
        {
            if (swapRequest.Status == SwapRequestStatus.Pending)
            {
                swapRequest.Status = SwapRequestStatus.Cancelled;
            }
            _context.SwapRequests.Remove(swapRequest);
        }

        _context.Shifts.Remove(shift);
        await _context.SaveChangesAsync(cancellationToken);

        return true;
    }
}