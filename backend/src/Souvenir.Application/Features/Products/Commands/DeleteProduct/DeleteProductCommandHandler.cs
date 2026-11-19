using MediatR;
using Microsoft.EntityFrameworkCore;
using Souvenir.Application.DTOs.Products;
using Souvenir.Application.Interfaces;

namespace Souvenir.Application.Features.Products.Commands.DeleteProduct;

public class DeleteProductCommandHandler : IRequestHandler<DeleteProductCommand, ProductResult<string>>
{
    private readonly IApplicationDbContext _context;

    public DeleteProductCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<ProductResult<string>> Handle(DeleteProductCommand command, CancellationToken cancellationToken)
    {
        var product = await _context.SanPhams
            .FirstOrDefaultAsync(p => p.Id == command.Id, cancellationToken);

        if (product == null)
        {
            return ProductResult<string>.Failed($"Không tìm thấy sản phẩm với mã #{command.Id}.", 404);
        }

        // Bước 60: Kiểm tra lịch sử giao dịch bán hàng của sản phẩm
        var hasSales = await _context.ChiTietDonHangs
            .AnyAsync(ct => ct.SanPhamId == product.Id, cancellationToken);

        var hasReturns = await _context.ChiTietTraHangs
            .AnyAsync(ct => ct.SanPhamId == product.Id, cancellationToken);

        var hasReceipts = await _context.ChiTietNhapKhos
            .AnyAsync(ct => ct.SanPhamId == product.Id, cancellationToken);

        if (hasSales || hasReturns || hasReceipts)
        {
            // Bước 66: Chặn xóa vật lý và chuyển trạng thái sang "Ngừng kinh doanh"
            product.TrangThai = "NgungKinhDoanh";
            await _context.SaveChangesAsync(cancellationToken);

            return ProductResult<string>.Success(
                "NgungKinhDoanh",
                "Sản phẩm đã có lịch sử giao dịch nên đã được chuyển sang trạng thái 'Ngừng kinh doanh'.");
        }
        else
        {
            // Bước 73: Xóa sản phẩm khỏi cơ sở dữ liệu
            _context.SanPhams.Remove(product);
            await _context.SaveChangesAsync(cancellationToken);

            return ProductResult<string>.Success(
                "DaXoa",
                "Xóa sản phẩm khỏi hệ thống thành công.");
        }
    }
}
