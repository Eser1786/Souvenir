using MediatR;
using Souvenir.Application.DTOs.Users;

namespace Souvenir.Application.Features.Users.Queries.GetRoles;

public record GetRolesQuery() : IRequest<List<RoleDto>>;
