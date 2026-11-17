using MediatR;
using Microsoft.EntityFrameworkCore;
using Souvenir.Application.DTOs.Users;
using Souvenir.Application.Interfaces;
using Souvenir.Domain.Constants;

namespace Souvenir.Application.Features.Users.Commands.DeleteUser;

public class DeleteUserCommandHandler : IRequestHandler<DeleteUserCommand, UserOperationResult>
{
    private readonly IApplicationDbContext _context;

    public DeleteUserCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<UserOperationResult> Handle(DeleteUserCommand command, CancellationToken cancellationToken)
    {
        var user = await _context.NguoiDungs
            .Include(u => u.VaiTro)
            .FirstOrDefaultAsync(u => u.Id == command.Id, cancellationToken);

        if (user == null)
        {
            return UserOperationResult.Failure("Không tìm thấy người dùng.", 404);
        }

        // Ràng buộc nghiệp vụ: Không cho phép xóa Quản lý đang hoạt động cuối cùng của hệ thống
        bool isCurrentlyActiveManager = user.VaiTro?.TenVaiTro == VaiTroConstants.QuanLy
                                     && user.TrangThai == TrangThaiTaiKhoan.HoatDong;

        if (isCurrentlyActiveManager)
        {
            var otherActiveManagersCount = await _context.NguoiDungs
                .CountAsync(u => u.Id != user.Id
                              && u.VaiTro != null
                              && u.VaiTro.TenVaiTro == VaiTroConstants.QuanLy
                              && u.TrangThai == TrangThaiTaiKhoan.HoatDong,
                            cancellationToken);

            if (otherActiveManagersCount == 0)
            {
                return UserOperationResult.Failure("Không thể xóa tài khoản Quản lý đang hoạt động cuối cùng của hệ thống.", 400);
            }
        }

        _context.NguoiDungs.Remove(user);
        await _context.SaveChangesAsync(cancellationToken);

        return UserOperationResult.SuccessResult(200, "Xóa tài khoản người dùng thành công.");
    }
}
