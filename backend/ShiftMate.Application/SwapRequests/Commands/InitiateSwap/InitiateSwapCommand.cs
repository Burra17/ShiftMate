using System.Text.Json.Serialization;
using MediatR;

namespace ShiftMate.Application.SwapRequests.Commands.InitiateSwap;

// Kommandot för att initiera en bytesförfrågan. Det innehåller ShiftId för det pass som ska bytas, och RequestingUserId som hämtas från token (och ignoreras i Swagger).
public record InitiateSwapCommand : IRequest<Guid>
{
    public Guid ShiftId { get; set; }

    [JsonIgnore]
    public Guid RequestingUserId { get; set; }
}
