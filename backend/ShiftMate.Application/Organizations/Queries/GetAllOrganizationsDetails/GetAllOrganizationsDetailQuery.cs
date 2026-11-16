using MediatR;
using ShiftMate.Application.DTOs;

namespace ShiftMate.Application.Organizations.Queries.GetAllOrganizationsDetails;

// Query för att hämta detaljerad information om alla organisationer, inklusive antal användare och pass.
public record GetAllOrganizationsDetailQuery : IRequest<List<OrganizationDetailDto>>;
