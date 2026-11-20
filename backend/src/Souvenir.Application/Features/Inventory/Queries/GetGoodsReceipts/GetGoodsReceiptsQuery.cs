using MediatR;
using Microsoft.EntityFrameworkCore;
using Souvenir.Application.DTOs.Inventory;
using Souvenir.Application.Interfaces;

namespace Souvenir.Application.Features.Inventory.Queries.GetGoodsReceipts;

public record GetGoodsReceiptsQuery : IRequest<List<GoodsReceiptDto>>;

public class GetGoodsReceiptsQueryHandler : IRequestHandler<GetGoodsReceiptsQuery, List<GoodsReceiptDto>>
{
    private readonly IApplicationDbContext _context;

    public GetGoodsReceiptsQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<GoodsReceiptDto>> Handle(GetGoodsReceiptsQuery request, CancellationToken cancellationToken)
    {
        var receipts = await _context.PhieuNhapKhos
            .AsNoTracking()
            .Include(p => p.NhaCungCap)
            .Include(p => p.NguoiDung)
            .Include(p => p.ChiTietNhapKhos)
                .ThenInclude(ct => ct.SanPham)
                    .ThenInclude(sp => sp!.DonViTinh)
            .OrderByDescending(p => p.NgayNhap)
            .Select(p => new GoodsReceiptDto
            {
                Id = p.Id,
                MaPhieu = p.MaPhieu,
                NhaCungCapId = p.NhaCungCapId,
                TenNCC = p.NhaCungCap != null ? p.NhaCungCap.TenNCC : string.Empty,
                NguoiDungId = p.NguoiDungId,
                TenNguoiNhap = p.NguoiDung != null ? p.NguoiDung.HoTen : string.Empty,
                TongTienNhap = p.TongTienNhap,
                NgayNhap = p.NgayNhap,
                ChiTiet = p.ChiTietNhapKhos.Select(ct => new GoodsReceiptDetailDto
                {
                    SanPhamId = ct.SanPhamId,
                    MaSP = ct.SanPham != null ? ct.SanPham.MaSP : string.Empty,
                    TenSP = ct.SanPham != null ? ct.SanPham.TenSP : string.Empty,
                    TenDonVi = ct.SanPham != null && ct.SanPham.DonViTinh != null ? ct.SanPham.DonViTinh.TenDonVi : null,
                    SoLuongNhap = ct.SoLuongNhap,
                    DonGiaNhap = ct.DonGiaNhap,
                    ThanhTien = ct.ThanhTien,
                    SoLuongTonMoi = ct.SanPham != null ? ct.SanPham.SoLuongTon : 0
                }).ToList()
            })
            .ToListAsync(cancellationToken);

        return receipts;
    }
}
