using MediatR;
using ShiftMate.Application.Common;
using ShiftMate.Application.DTOs;

namespace ShiftMate.Application.Users.Queries.GetAllUsers;

// Datan som behövs för att hämta alla användare i en organisation, med stöd för paginering.
public record GetAllUsersQuery(
    Guid OrganizationId,
    int? Page = null,
    int? PageSize = null) : IRequest<PagedResult<UserDto>>;
