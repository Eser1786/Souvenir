using MediatR;
using Microsoft.EntityFrameworkCore;
using Souvenir.Application.DTOs.Products;
using Souvenir.Application.Interfaces;

namespace Souvenir.Application.Features.Products.Queries.GetProductById;

public record GetProductByIdQuery(int Id) : IRequest<ProductDto?>;

public class GetProductByIdQueryHandler : IRequestHandler<GetProductByIdQuery, ProductDto?>
{
    private readonly IApplicationDbContext _context;

    public GetProductByIdQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<ProductDto?> Handle(GetProductByIdQuery request, CancellationToken cancellationToken)
    {
        var p = await _context.SanPhams
            .AsNoTracking()
            .Include(x => x.DanhMuc)
            .Include(x => x.DonViTinh)
            .FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken);

        if (p == null) return null;

        return new ProductDto
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
        };
    }
}
