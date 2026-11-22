using MediatR;
using Souvenir.Application.DTOs.Auth;

namespace Souvenir.Application.Features.Auth.Commands.Logout;

public record LogoutCommand(int? UserId = null) : IRequest<LogoutResult>;
