using MediatR;
using Microsoft.EntityFrameworkCore;
using ShiftMate.Application.Interfaces;

namespace ShiftMate.Application.Organizations.Commands.DeleteOrganization;
// Command för att radera en organisation.
public record DeleteOrganizationCommand(Guid Id) : IRequest;
