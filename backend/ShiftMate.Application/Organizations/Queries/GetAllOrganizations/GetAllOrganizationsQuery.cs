using MediatR;
using ShiftMate.Application.DTOs;

namespace ShiftMate.Application.Organizations.Queries.GetAllOrganizations;

// Query för att hämta alla organisationer.
public record GetAllOrganizationsQuery : IRequest<List<OrganizationDto>>;
