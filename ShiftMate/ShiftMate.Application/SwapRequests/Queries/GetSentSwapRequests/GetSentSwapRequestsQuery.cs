using MediatR;
using ShiftMate.Application.DTOs;
using System.Text.Json.Serialization;

namespace ShiftMate.Application.SwapRequests.Queries.GetSentSwapRequests;

// Hämtar bytesförfrågningar som den inloggade användaren har skickat.
public record GetSentSwapRequestsQuery : IRequest<List<SwapRequestDto>>
{
    [JsonIgnore]
    public Guid CurrentUserId { get; set; }
}
