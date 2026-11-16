using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using ShiftMate.Application.Interfaces;

namespace ShiftMate.Application.Organizations.Commands.UpdateOrganization;

public record UpdateOrganizationCommand(Guid Id, string Name) : IRequest;
