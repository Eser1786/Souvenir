using MediatR;
using Microsoft.EntityFrameworkCore;
using Souvenir.Application.DTOs.Inventory;
using Souvenir.Application.Interfaces;

namespace Souvenir.Application.Features.Inventory.Queries.GetSuppliers;

public record GetSuppliersQuery : IRequest<List<SupplierDto>>;

public class GetSuppliersQueryHandler : IRequestHandler<GetSuppliersQuery, List<SupplierDto>>
{
    private readonly IApplicationDbContext _context;

    public GetSuppliersQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<SupplierDto>> Handle(GetSuppliersQuery request, CancellationToken cancellationToken)
    {
        var suppliers = await _context.NhaCungCaps
            .AsNoTracking()
            .OrderBy(s => s.Id)
            .Select(s => new SupplierDto
            {
                Id = s.Id,
                MaNCC = s.MaNCC,
                TenNCC = s.TenNCC,
                SoDienThoai = s.SoDienThoai,
                DiaChi = s.DiaChi
            })
            .ToListAsync(cancellationToken);

        // Fallback demo data if DB table has not run seed yet
        if (suppliers.Count == 0)
        {
            return new List<SupplierDto>
            {
                new() { Id = 1, MaNCC = "NCC001", TenNCC = "Cơ sở quà tặng Việt An", SoDienThoai = "0900000001", DiaChi = "Quận 5, TP. Hồ Chí Minh" },
                new() { Id = 2, MaNCC = "NCC002", TenNCC = "Cơ sở thủ công Mỹ Nghệ Thành Đạt", SoDienThoai = "0900000002", DiaChi = "Quận 6, TP. Hồ Chí Minh" },
                new() { Id = 3, MaNCC = "NCC003", TenNCC = "Nhà phân phối Văn Phòng Phẩm Minh Châu", SoDienThoai = "0900000003", DiaChi = "Quận 10, TP. Hồ Chí Minh" },
                new() { Id = 4, MaNCC = "NCC004", TenNCC = "Xưởng may Quà Tặng Sao Việt", SoDienThoai = "0900000004", DiaChi = "Quận Tân Bình, TP. Hồ Chí Minh" },
            };
        }

        return suppliers;
    }
}
