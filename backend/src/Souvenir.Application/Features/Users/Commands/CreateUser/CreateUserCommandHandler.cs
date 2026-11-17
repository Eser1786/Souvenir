using MediatR;
using Microsoft.EntityFrameworkCore;
using Souvenir.Application.DTOs.Users;
using Souvenir.Application.Interfaces;
using Souvenir.Domain.Constants;
using Souvenir.Domain.Entities;

namespace Souvenir.Application.Features.Users.Commands.CreateUser;

public class CreateUserCommandHandler : IRequestHandler<CreateUserCommand, UserOperationResult<UserDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly IPasswordHasher _passwordHasher;

    public CreateUserCommandHandler(IApplicationDbContext context, IPasswordHasher passwordHasher)
    {
        _context = context;
        _passwordHasher = passwordHasher;
    }

    public async Task<UserOperationResult<UserDto>> Handle(CreateUserCommand command, CancellationToken cancellationToken)
    {
        var req = command.Request;
        if (req == null)
        {
            return UserOperationResult<UserDto>.Failure("Dữ liệu yêu cầu không hợp lệ.", 400);
        }

        if (string.IsNullOrWhiteSpace(req.TenDangNhap))
        {
            return UserOperationResult<UserDto>.Failure("Tên đăng nhập không được để trống.", 400);
        }

        if (string.IsNullOrWhiteSpace(req.MatKhau))
        {
            return UserOperationResult<UserDto>.Failure("Mật khẩu không được để trống.", 400);
        }

        if (string.IsNullOrWhiteSpace(req.HoTen))
        {
            return UserOperationResult<UserDto>.Failure("Họ và tên không được để trống.", 400);
        }

        var role = await _context.VaiTros
            .FirstOrDefaultAsync(r => r.Id == req.VaiTroId, cancellationToken);
        if (role == null)
        {
            return UserOperationResult<UserDto>.Failure("Vai trò được chọn không tồn tại trong hệ thống.", 400);
        }

        var normalizedUsername = req.TenDangNhap.Trim().ToLower();
        var isDuplicate = await _context.NguoiDungs
            .AnyAsync(u => u.TenDangNhap.ToLower() == normalizedUsername, cancellationToken);
        if (isDuplicate)
        {
            return UserOperationResult<UserDto>.Failure($"Tên đăng nhập '{req.TenDangNhap.Trim()}' đã tồn tại trong hệ thống.", 409);
        }

        var hashedPassword = _passwordHasher.HashPassword(req.MatKhau);

        var newUser = new NguoiDung
        {
            TenDangNhap = req.TenDangNhap.Trim(),
            MatKhau = hashedPassword,
            HoTen = req.HoTen.Trim(),
            SoDienThoai = req.SoDienThoai?.Trim() ?? string.Empty,
            VaiTroId = role.Id,
            VaiTro = role,
            TrangThai = TrangThaiTaiKhoan.HoatDong,
            SoLanDangNhapSai = 0
        };

        _context.NguoiDungs.Add(newUser);
        await _context.SaveChangesAsync(cancellationToken);

        return UserOperationResult<UserDto>.Success(UserDto.FromEntity(newUser), 201, "Tạo tài khoản thành công.");
    }
}
