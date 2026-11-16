using MediatR;

namespace ShiftMate.Application.Shifts.Commands.DeleteShift;

// Command för att radera ett pass.
public record DeleteShiftCommand(Guid ShiftId, Guid OrganizationId) : IRequest<bool>;
