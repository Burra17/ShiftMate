using MediatR;
using System.Text.Json.Serialization;

namespace ShiftMate.Application.Shifts.Commands.TakeShift;

// Command för att ta ett pass. Används när en användare vill ta ett pass som är tillgängligt.
public class TakeShiftCommand : IRequest<bool>
{
    public Guid ShiftId { get; set; }
    public Guid UserId { get; set; }

    [JsonIgnore]
    public Guid OrganizationId { get; set; }
}
