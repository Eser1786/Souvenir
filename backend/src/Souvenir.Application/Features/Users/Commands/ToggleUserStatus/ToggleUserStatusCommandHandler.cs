using MediatR;
using Microsoft.EntityFrameworkCore;
using Souvenir.Application.DTOs.Users;
using Souvenir.Application.Interfaces;
using Souvenir.Domain.Constants;

namespace Souvenir.Application.Features.Users.Commands.ToggleUserStatus;

public class ToggleUserStatusCommandHandler : IRequestHandler<ToggleUserStatusCommand, UserOperationResult<UserDto>>
{
    private readonly IApplicationDbContext _context;

    public ToggleUserStatusCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<UserOperationResult<UserDto>> Handle(ToggleUserStatusCommand command, CancellationToken cancellationToken)
    {
        var user = await _context.NguoiDungs
            .Include(u => u.VaiTro)
            .FirstOrDefaultAsync(u => u.Id == command.Id, cancellationToken);

        if (user == null)
        {
            return UserOperationResult<UserDto>.Failure("Không tìm thấy người dùng.", 404);
        }

        string targetStatus;
        if (!string.IsNullOrWhiteSpace(command.TrangThai))
        {
            var requestedStatus = command.TrangThai.Trim();
            if (requestedStatus != TrangThaiTaiKhoan.HoatDong && requestedStatus != TrangThaiTaiKhoan.Khoa)
            {
                return UserOperationResult<UserDto>.Failure("Trạng thái không hợp lệ. Chỉ chấp nhận 'HoatDong' hoặc 'Khoa'.", 400);
            }
            targetStatus = requestedStatus;
        }
        else
        {
            targetStatus = user.TrangThai == TrangThaiTaiKhoan.HoatDong
                ? TrangThaiTaiKhoan.Khoa
                : TrangThaiTaiKhoan.HoatDong;
        }

        // Kiểm tra ràng buộc: Không cho phép khóa tài khoản Quản lý đang hoạt động cuối cùng của hệ thống
        bool isCurrentlyActiveManager = user.VaiTro?.TenVaiTro == VaiTroConstants.QuanLy
                                     && user.TrangThai == TrangThaiTaiKhoan.HoatDong;
        bool isLocking = targetStatus == TrangThaiTaiKhoan.Khoa;

        if (isCurrentlyActiveManager && isLocking)
        {
            var otherActiveManagersCount = await _context.NguoiDungs
                .CountAsync(u => u.Id != user.Id
                              && u.VaiTro != null
                              && u.VaiTro.TenVaiTro == VaiTroConstants.QuanLy
                              && u.TrangThai == TrangThaiTaiKhoan.HoatDong,
                            cancellationToken);

            if (otherActiveManagersCount == 0)
            {
                return UserOperationResult<UserDto>.Failure("Không thể khóa tài khoản Quản lý đang hoạt động cuối cùng của hệ thống.", 400);
            }
        }

        user.TrangThai = targetStatus;

        // Khi mở khóa hoặc kích hoạt lại tài khoản, đặt lại số lần đăng nhập sai về 0
        if (targetStatus == TrangThaiTaiKhoan.HoatDong)
        {
            user.SoLanDangNhapSai = 0;
        }

        await _context.SaveChangesAsync(cancellationToken);

        return UserOperationResult<UserDto>.Success(
            UserDto.FromEntity(user),
            200,
            $"Cập nhật trạng thái tài khoản thành '{targetStatus}' thành công.");
    }
}
