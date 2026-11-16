using MediatR;

namespace ShiftMate.Application.Shifts.Commands.CancelShiftSwap;

// Command för att avbryta en bytesförfrågan för ett pass. Används av både den som begärt bytet och den som mottagit bytet.
public class CancelShiftSwapCommand : IRequest<bool>
{
    public Guid ShiftId { get; set; }
    public Guid UserId { get; set; } // Kommer från token
}
