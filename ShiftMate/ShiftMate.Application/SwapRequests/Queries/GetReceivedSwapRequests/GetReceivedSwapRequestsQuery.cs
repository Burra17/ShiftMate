using MediatR;
using ShiftMate.Application.DTOs;
using System.Text.Json.Serialization;

namespace ShiftMate.Application.SwapRequests.Queries.GetReceivedSwapRequests;

// Datan som behövs för att hämta inkommande bytesförfrågningar.
// CurrentUserId kommer att sättas i controllern från JWT-token.
public record GetReceivedSwapRequestsQuery : IRequest<List<SwapRequestDto>>
{
    [JsonIgnore]
    public Guid CurrentUserId { get; set; }
}
