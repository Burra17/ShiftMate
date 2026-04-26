using MediatR;
using System.Text.Json.Serialization;

namespace ShiftMate.Application.SwapRequests.Commands.AcceptSwap;

// Kommandot för att acceptera en bytesförfrågan.
public record AcceptSwapCommand : IRequest
{
    public Guid SwapRequestId { get; set; }

    [JsonIgnore] // Vi hämtar detta från token, så Swagger ska inte visa det
    public Guid CurrentUserId { get; set; }
}
﻿﻿