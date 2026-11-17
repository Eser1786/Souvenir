using MediatR;
using Microsoft.EntityFrameworkCore;
using Souvenir.Application.DTOs.Users;
using Souvenir.Application.Interfaces;
using Souvenir.Domain.Constants;

namespace Souvenir.Application.Features.Users.Commands.UpdateUser;

public class UpdateUserCommandHandler : IRequestHandler<UpdateUserCommand, UserOperationResult<UserDto>>
{
    private readonly IApplicationDbContext _context;

    public UpdateUserCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<UserOperationResult<UserDto>> Handle(UpdateUserCommand command, CancellationToken cancellationToken)
    {
        var req = command.Request;
        if (req == null)
        {
            return UserOperationResult<UserDto>.Failure("Dữ liệu yêu cầu không hợp lệ.", 400);
        }

        if (string.IsNullOrWhiteSpace(req.HoTen))
        {
            return UserOperationResult<UserDto>.Failure("Họ và tên không được để trống.", 400);
        }

        var user = await _context.NguoiDungs
            .Include(u => u.VaiTro)
            .FirstOrDefaultAsync(u => u.Id == command.Id, cancellationToken);

        if (user == null)
        {
            return UserOperationResult<UserDto>.Failure("Không tìm thấy người dùng.", 404);
        }

        var targetRole = await _context.VaiTros
            .FirstOrDefaultAsync(r => r.Id == req.VaiTroId, cancellationToken);

        if (targetRole == null)
        {
            return UserOperationResult<UserDto>.Failure("Vai trò được chọn không tồn tại trong hệ thống.", 400);
        }

        // Kiểm tra ràng buộc: Không được hạ quyền Quản lý đang hoạt động cuối cùng của hệ thống
        bool isCurrentlyActiveManager = user.VaiTro?.TenVaiTro == VaiTroConstants.QuanLy
                                     && user.TrangThai == TrangThaiTaiKhoan.HoatDong;
        bool isChangingAwayFromManager = isCurrentlyActiveManager
                                      && targetRole.TenVaiTro != VaiTroConstants.QuanLy;

        if (isChangingAwayFromManager)
        {
            var otherActiveManagersCount = await _context.NguoiDungs
                .CountAsync(u => u.Id != user.Id
                              && u.VaiTro != null
                              && u.VaiTro.TenVaiTro == VaiTroConstants.QuanLy
                              && u.TrangThai == TrangThaiTaiKhoan.HoatDong,
                            cancellationToken);

            if (otherActiveManagersCount == 0)
            {
                return UserOperationResult<UserDto>.Failure("Không thể hạ quyền tài khoản Quản lý đang hoạt động cuối cùng của hệ thống.", 400);
            }
        }

        user.HoTen = req.HoTen.Trim();
        user.SoDienThoai = req.SoDienThoai?.Trim() ?? string.Empty;
        user.VaiTroId = targetRole.Id;
        user.VaiTro = targetRole;

        await _context.SaveChangesAsync(cancellationToken);

        return UserOperationResult<UserDto>.Success(UserDto.FromEntity(user), 200, "Cập nhật thông tin người dùng thành công.");
    }
}
