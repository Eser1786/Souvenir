using MediatR;
using Microsoft.EntityFrameworkCore;
using Souvenir.Application.DTOs.Orders;
using Souvenir.Application.Interfaces;
using Souvenir.Domain.Entities;

namespace Souvenir.Application.Features.DraftOrders.Commands.DeleteDraftOrder;

public class DeleteDraftOrderCommandHandler : IRequestHandler<DeleteDraftOrderCommand, OrderOperationResult<bool>>
{
    private readonly IApplicationDbContext _context;

    public DeleteDraftOrderCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<OrderOperationResult<bool>> Handle(DeleteDraftOrderCommand command, CancellationToken cancellationToken)
    {
        var order = await _context.DonHangs
            .Include(o => o.ChiTietDonHangs)
            .FirstOrDefaultAsync(o => o.Id == command.Id && (o.TrangThai == TrangThaiDonHang.LuuNhap || o.TrangThai == "LuuNhap"), cancellationToken);

        if (order == null)
        {
            return OrderOperationResult<bool>.Failure("Không tìm thấy đơn nháp hoặc đơn không ở trạng thái lưu nháp.", 404);
        }

        // Xóa các chi tiết đơn nháp
        if (order.ChiTietDonHangs.Any())
        {
            _context.ChiTietDonHangs.RemoveRange(order.ChiTietDonHangs);
        }

        // Xóa đơn hàng
        _context.DonHangs.Remove(order);
        await _context.SaveChangesAsync(cancellationToken);

        return OrderOperationResult<bool>.Success(true, 200, "Xóa đơn nháp thành công.");
    }
}
