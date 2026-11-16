using MediatR;
using Souvenir.Application.DTOs.Auth;

namespace Souvenir.Application.Features.Auth.Commands.Login;

public record LoginCommand(string TenDangNhap, string MatKhau) : IRequest<AuthResult>;
