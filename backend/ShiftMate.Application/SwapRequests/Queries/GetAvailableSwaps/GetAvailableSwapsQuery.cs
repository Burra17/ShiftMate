using MediatR;
using ShiftMate.Application.DTOs;

namespace ShiftMate.Application.SwapRequests.Queries.GetAvailableSwaps;

// Queryn för att hämta alla tillgängliga bytesförfrågningar i en organisation.
// Den kräver bara organisationens ID och returnerar en lista av SwapRequestDto som innehåller relevant information om varje förfrågan, inklusive pass och användare.
public record GetAvailableSwapsQuery(Guid OrganizationId) : IRequest<List<SwapRequestDto>>;
