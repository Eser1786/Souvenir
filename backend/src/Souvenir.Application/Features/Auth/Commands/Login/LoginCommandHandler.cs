using MediatR;
using Microsoft.EntityFrameworkCore;
using Souvenir.Application.DTOs.Auth;
using Souvenir.Application.Interfaces;
using Souvenir.Domain.Constants;

namespace Souvenir.Application.Features.Auth.Commands.Login;

public class LoginCommandHandler : IRequestHandler<LoginCommand, AuthResult>
{
    private readonly IApplicationDbContext _context;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IJwtTokenGenerator _jwtTokenGenerator;

    public LoginCommandHandler(
        IApplicationDbContext context,
        IPasswordHasher passwordHasher,
        IJwtTokenGenerator jwtTokenGenerator)
    {
        _context = context;
        _passwordHasher = passwordHasher;
        _jwtTokenGenerator = jwtTokenGenerator;
    }

    public async Task<AuthResult> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.TenDangNhap) || string.IsNullOrWhiteSpace(request.MatKhau))
        {
            return AuthResult.Failed("Tên đăng nhập hoặc mật khẩu không chính xác.", statusCode: 401);
        }

        var maxAttempts = await GetMaxLoginAttemptsAsync(cancellationToken);

        var user = await _context.NguoiDungs
            .Include(u => u.VaiTro)
            .FirstOrDefaultAsync(u => u.TenDangNhap == request.TenDangNhap.Trim(), cancellationToken);

        if (user == null)
        {
            return AuthResult.Failed("Tên đăng nhập hoặc mật khẩu không chính xác.", statusCode: 401);
        }

        if (string.Equals(user.TrangThai, TrangThaiTaiKhoan.Khoa, StringComparison.OrdinalIgnoreCase))
        {
            return AuthResult.Locked($"Tài khoản tạm khóa sau {maxAttempts} lần nhập sai. Vui lòng liên hệ quản lý để được mở khóa.");
        }

        bool isPasswordValid = _passwordHasher.VerifyPassword(request.MatKhau, user.MatKhau);

        if (!isPasswordValid)
        {
            user.SoLanDangNhapSai++;

            if (user.SoLanDangNhapSai >= maxAttempts)
            {
                user.TrangThai = TrangThaiTaiKhoan.Khoa;
                await _context.SaveChangesAsync(cancellationToken);

                return AuthResult.Locked($"Tài khoản tạm khóa sau {maxAttempts} lần nhập sai. Vui lòng liên hệ quản lý để được mở khóa.");
            }

            await _context.SaveChangesAsync(cancellationToken);
            int remaining = maxAttempts - user.SoLanDangNhapSai;
            return AuthResult.Failed(
                $"Mật khẩu không chính xác. Bạn còn {remaining} lần thử.",
                statusCode: 401,
                remainingAttempts: remaining);
        }

        // Reset wrong attempts on successful login
        user.SoLanDangNhapSai = 0;
        await _context.SaveChangesAsync(cancellationToken);

        var (token, expiresIn) = _jwtTokenGenerator.GenerateToken(user);

        var userDto = new UserDto
        {
            Id = user.Id,
            TenDangNhap = user.TenDangNhap,
            HoTen = user.HoTen,
            SoDienThoai = user.SoDienThoai?.Trim() ?? string.Empty,
            VaiTro = user.VaiTro?.TenVaiTro ?? string.Empty,
            TrangThai = user.TrangThai
        };

        var response = new LoginResponse
        {
            Token = token,
            TokenType = "Bearer",
            ExpiresIn = expiresIn,
            User = userDto
        };

        return AuthResult.Success(response);
    }

    private async Task<int> GetMaxLoginAttemptsAsync(CancellationToken cancellationToken)
    {
        const int defaultMaxAttempts = 5;
        var param = await _context.ThamSos
            .AsNoTracking()
            .FirstOrDefaultAsync(t => t.TenThamSo == ThamSoConstants.SoLanDangNhapSaiToiDa, cancellationToken);

        if (param != null && int.TryParse(param.GiaTri, out var max) && max > 0)
        {
            return max;
        }

        return defaultMaxAttempts;
    }
}
