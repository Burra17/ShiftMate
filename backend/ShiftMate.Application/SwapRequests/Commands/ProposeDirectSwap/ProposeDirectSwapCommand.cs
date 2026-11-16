using System.Text.Json.Serialization;
using MediatR;

namespace ShiftMate.Application.SwapRequests.Commands.ProposeDirectSwap;

// Kommandot: Definierar vilken data som krävs för att starta ett direktbyte
public record ProposeDirectSwapCommand : IRequest<Guid>
{
    public Guid MyShiftId { get; set; }     // Passet användaren vill bli av med
    public Guid TargetShiftId { get; set; } // Passet användaren vill ha istället

    [JsonIgnore]
    public Guid RequestingUserId { get; set; }

    [JsonIgnore]
    public Guid OrganizationId { get; set; }
}
