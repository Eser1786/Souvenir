using MediatR;
using Microsoft.EntityFrameworkCore;
using Souvenir.Application.DTOs.Products;
using Souvenir.Application.Interfaces;

namespace Souvenir.Application.Features.Products.Queries.GetProducts;

public class GetProductsQueryHandler : IRequestHandler<GetProductsQuery, List<ProductDto>>
{
    private readonly IApplicationDbContext _context;

    public GetProductsQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<ProductDto>> Handle(GetProductsQuery request, CancellationToken cancellationToken)
    {
        var query = _context.SanPhams
            .AsNoTracking()
            .Include(p => p.DanhMuc)
            .Include(p => p.DonViTinh)
            .AsQueryable();

        if (!request.IncludeAllStatus)
        {
            query = query.Where(p => p.TrangThai == "KinhDoanh");
        }

        if (!string.IsNullOrWhiteSpace(request.Category) &&
            !request.Category.Equals("Tất cả", StringComparison.OrdinalIgnoreCase) &&
            !request.Category.Equals("Tat ca", StringComparison.OrdinalIgnoreCase) &&
            !request.Category.Equals("All", StringComparison.OrdinalIgnoreCase))
        {
            var categoryFilter = request.Category.Trim();
            string? matchedDbCategory = categoryFilter.ToLower() switch
            {
                "quà tặng" or "qua tang" => "Qua tang",
                "thú bông" or "thu bong" => "Thu bong",
                "trang trí" or "đồ trang trí" or "do trang tri" => "Do trang tri",
                "văn phòng phẩm" or "van phong pham" => "Van phong pham",
                "đồ lưu niệm" or "do luu niem" => "Do luu niem",
                "móc khóa" or "moc khoa" => "Moc khoa",
                "đồ thủ công mỹ nghệ" or "do thu cong my nghe" => "Do thu cong my nghe",
                "trang sức và phụ kiện" or "trang suc va phu kien" => "Trang suc va phu kien",
                "áo thun và túi vải" or "ao thun va tui vai" => "Ao thun va tui vai",
                _ => null
            };

            if (matchedDbCategory != null)
            {
                query = query.Where(p => p.DanhMuc != null && p.DanhMuc.TenDanhMuc == matchedDbCategory);
            }
            else
            {
                var lower = categoryFilter.ToLower();
                query = query.Where(p => p.DanhMuc != null && p.DanhMuc.TenDanhMuc.ToLower().Contains(lower));
            }
        }

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var searchTerm = request.Search.Trim().ToLower();
            query = query.Where(p => p.TenSP.ToLower().Contains(searchTerm) || p.MaSP.ToLower().Contains(searchTerm));
        }

        var result = await query
            .OrderBy(p => p.MaSP == "GB001" ? 1 :
                          p.MaSP == "LY002" ? 2 :
                          p.MaSP == "DN003" ? 3 :
                          p.MaSP == "HQ004" ? 4 :
                          p.MaSP == "BT005" ? 5 :
                          p.MaSP == "DL006" ? 6 : 100)
            .ThenBy(p => p.Id)
            .Select(p => new ProductDto
            {
                Id = p.Id,
                MaSP = p.MaSP,
                TenSP = p.TenSP,
                DanhMucId = p.DanhMucId,
                TenDanhMuc = p.DanhMuc != null ? p.DanhMuc.TenDanhMuc : string.Empty,
                DonViTinhId = p.DonViTinhId,
                TenDonVi = p.DonViTinh != null ? p.DonViTinh.TenDonVi : null,
                GiaNhap = p.GiaNhap,
                GiaBan = p.GiaBan,
                SoLuongTon = p.SoLuongTon,
                TrangThai = p.TrangThai,
                HinhAnh = p.HinhAnh
            })
            .ToListAsync(cancellationToken);

        return result;
    }
}
