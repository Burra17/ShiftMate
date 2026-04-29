using MediatR;
using Microsoft.EntityFrameworkCore;
using ShiftMate.Application.Common.Exceptions;
using ShiftMate.Application.Interfaces;
using ShiftMate.Domain.Entities;
using ShiftMate.Domain.Enums;

namespace ShiftMate.Application.SwapRequests.Commands.InitiateSwap;

// Handlern för att initiera en bytesförfrågan. Den säkerställer att användaren äger passet, skapar en ny förfrågan och markerar passet som "Ute för byte".
public class InitiateSwapCommandHandler : IRequestHandler<InitiateSwapCommand, Guid>
{
    private readonly IAppDbContext _context;

    public InitiateSwapCommandHandler(IAppDbContext context)
    {
        _context = context;
    }

    public async Task<Guid> Handle(InitiateSwapCommand request, CancellationToken cancellationToken)
    {
        // A. Hämta passet
        var shift = await _context.Shifts
            .FirstOrDefaultAsync(s => s.Id == request.ShiftId, cancellationToken);

        if (shift == null)
        {
            throw new NotFoundException("Passet hittades inte.");
        }

        // B. Säkerhetskoll: Äger du verkligen det här passet?
        if (shift.UserId != request.RequestingUserId)
        {
            throw new ForbiddenException("Du kan inte byta bort någon annans pass!");
        }

        // C. Skapa förfrågan
        var swapRequest = new SwapRequest
        {
            Id = Guid.NewGuid(),
            ShiftId = request.ShiftId,
            RequestingUserId = request.RequestingUserId,
            Status = SwapRequestStatus.Pending,
            CreatedAt = DateTime.UtcNow
        };

        // D. Markera passet som "Ute för byte"
        shift.IsUpForSwap = true;

        // E. Spara allt
        _context.SwapRequests.Add(swapRequest);
        await _context.SaveChangesAsync(cancellationToken);

        return swapRequest.Id;
    }
}
