using MediatR;
using Souvenir.Application.DTOs.Users;

namespace Souvenir.Application.Features.Users.Commands.DeleteUser;

public record DeleteUserCommand(int Id) : IRequest<UserOperationResult>;
