using MediatR;

namespace ShiftMate.Application.Organizations.Commands.CreateOrganization;

public record CreateOrganizationCommand(string Name) : IRequest<Guid>;
