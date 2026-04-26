using MediatR;
using System.Text.Json.Serialization;

namespace ShiftMate.Application.SwapRequests.Commands.DeclineSwapRequest;

// Datan som behövs för att neka en bytesförfrågan.
// SwapRequestId kommer från URL:en, och CurrentUserId sätts i controllern.
public record DeclineSwapRequestCommand : IRequest
{
    public Guid SwapRequestId { get; set; }

    [JsonIgnore]
    public Guid CurrentUserId { get; set; }
}
