using MediatR;
using System.Text.Json.Serialization;

namespace ShiftMate.Application.Shifts.Commands.CreateShift;

// Command för att skapa ett nytt pass i en organisation. Returnerar det nya passets ID.
public record CreateShiftCommand : IRequest<Guid>
{
    public Guid? UserId { get; set; }
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }

    [JsonIgnore]
    public Guid OrganizationId { get; set; }
}
