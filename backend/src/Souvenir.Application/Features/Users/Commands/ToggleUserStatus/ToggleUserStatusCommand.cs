using MediatR;
using Souvenir.Application.DTOs.Users;

namespace Souvenir.Application.Features.Users.Commands.ToggleUserStatus;

public record ToggleUserStatusCommand(int Id, string? TrangThai = null) : IRequest<UserOperationResult<UserDto>>;
