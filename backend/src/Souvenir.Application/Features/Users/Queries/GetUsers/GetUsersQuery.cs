using MediatR;
using Souvenir.Application.DTOs.Users;

namespace Souvenir.Application.Features.Users.Queries.GetUsers;

public record GetUsersQuery(string? Keyword = null, int? VaiTroId = null, string? TrangThai = null) : IRequest<List<UserDto>>;
