using MediatR;

namespace ShiftMate.Application.Shifts.Commands
{
    public class TakeShiftCommand : IRequest<bool>
    {
        public Guid ShiftId { get; set; }
        public Guid UserId { get; set; } // Detta kommer från token i controllern
    }
}
