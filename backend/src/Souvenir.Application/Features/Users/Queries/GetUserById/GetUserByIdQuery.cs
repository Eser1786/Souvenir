using MediatR;
using Souvenir.Application.DTOs.Users;

namespace Souvenir.Application.Features.Users.Queries.GetUserById;

public record GetUserByIdQuery(int Id) : IRequest<UserDto?>;
