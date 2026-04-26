using MediatR;
using System.Text.Json.Serialization;

namespace ShiftMate.Application.Shifts.Commands.UpdateShift;

// Command för att uppdatera ett pass. Inkluderar valfri UserId för att tilldela eller avlägsna en användare från passet.
public record UpdateShiftCommand : IRequest<bool>
{
    public Guid ShiftId { get; set; }
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    public Guid? UserId { get; set; }

    [JsonIgnore]
    public Guid OrganizationId { get; set; }
}
