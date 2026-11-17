using MediatR;
using Souvenir.Application.DTOs.Users;

namespace Souvenir.Application.Features.Users.Commands.CreateUser;

public record CreateUserCommand(CreateUserRequest Request) : IRequest<UserOperationResult<UserDto>>;
