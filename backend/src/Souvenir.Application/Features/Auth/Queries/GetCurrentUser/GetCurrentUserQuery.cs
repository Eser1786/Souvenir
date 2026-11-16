using MediatR;
using Souvenir.Application.DTOs.Auth;

namespace Souvenir.Application.Features.Auth.Queries.GetCurrentUser;

public record GetCurrentUserQuery(int UserId) : IRequest<UserDto?>;
