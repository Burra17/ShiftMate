using MediatR;
using System;

namespace ShiftMate.Application.Shifts.Commands
{
    public class CancelShiftSwapCommand : IRequest<bool>
    {
        public Guid ShiftId { get; set; }
        public Guid UserId { get; set; } // Kommer från token
    }
}
