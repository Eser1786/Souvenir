using MediatR;
using Souvenir.Application.DTOs.Users;

namespace Souvenir.Application.Features.Users.Commands.UpdateUser;

public record UpdateUserCommand(int Id, UpdateUserRequest Request) : IRequest<UserOperationResult<UserDto>>;
