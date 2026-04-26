using MediatR;

namespace ShiftMate.Application.SwapRequests.Commands.CancelSwapRequest;

//Datan som behövs för att avbryta en bytesförfrågan. SwapRequestId kommer från URL:en, och CurrentUserId sätts i controllern.
public record CancelSwapRequestCommand(Guid SwapRequestId, Guid CurrentUserId) : IRequest;
