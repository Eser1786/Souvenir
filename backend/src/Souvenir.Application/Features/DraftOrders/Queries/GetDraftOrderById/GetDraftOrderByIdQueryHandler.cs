using MediatR;
using Microsoft.EntityFrameworkCore;
using Souvenir.Application.DTOs.Orders;
using Souvenir.Application.Interfaces;
using Souvenir.Domain.Entities;

namespace Souvenir.Application.Features.DraftOrders.Queries.GetDraftOrderById;

public class GetDraftOrderByIdQueryHandler : IRequestHandler<GetDraftOrderByIdQuery, DraftOrderDto?>
{
    private readonly IApplicationDbContext _context;

    public GetDraftOrderByIdQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<DraftOrderDto?> Handle(GetDraftOrderByIdQuery request, CancellationToken cancellationToken)
    {
        var o = await _context.DonHangs
            .AsNoTracking()
            .Where(x => x.Id == request.Id && (x.TrangThai == TrangThaiDonHang.LuuNhap || x.TrangThai == "LuuNhap"))
            .Include(x => x.ChiTietDonHangs)
            .Include(x => x.NguoiDung)
            .Include(x => x.MaGiamGia)
            .FirstOrDefaultAsync(cancellationToken);

        if (o == null) return null;

        var productIds = o.ChiTietDonHangs.Select(ct => ct.SanPhamId).Distinct().ToList();
        var products = await _context.SanPhams
            .AsNoTracking()
            .Where(p => productIds.Contains(p.Id))
            .ToDictionaryAsync(p => p.Id, cancellationToken);

        KhachHang? kh = null;
        if (o.KhachHangId.HasValue)
        {
            kh = await _context.KhachHangs.AsNoTracking().FirstOrDefaultAsync(k => k.Id == o.KhachHangId.Value, cancellationToken);
        }

        var detailDtos = new List<DraftOrderItemDto>();
        decimal tongTienHang = 0m;

        foreach (var ct in o.ChiTietDonHangs)
        {
            products.TryGetValue(ct.SanPhamId, out var sp);
            var itemThanhTien = ct.ThanhTien > 0 ? ct.ThanhTien : (sp != null ? sp.GiaBan * ct.SoLuong : 0m);
            tongTienHang += itemThanhTien;

            detailDtos.Add(new DraftOrderItemDto
            {
                SanPhamId = ct.SanPhamId,
                MaSP = sp?.MaSP ?? string.Empty,
                TenSP = sp?.TenSP ?? $"Sản phẩm #{ct.SanPhamId}",
                ImageUrl = sp?.HinhAnh,
                SoLuong = ct.SoLuong,
                DonGia = ct.DonGia > 0 ? ct.DonGia : (sp?.GiaBan ?? 0m),
                ThanhTien = itemThanhTien,
                SoLuongTon = sp?.SoLuongTon ?? 0
            });
        }

        decimal soTienGiam = Math.Max(0m, tongTienHang - o.TongTienThanhToan);
        decimal voucherGiam = o.MaGiamGia != null ? o.MaGiamGia.TinhTienGiam(tongTienHang) : 0m;
        decimal diemGiam = Math.Max(0m, soTienGiam - voucherGiam);
        int diemSuDung = (int)Math.Round(diemGiam / 100m);

        return new DraftOrderDto
        {
            Id = o.Id,
            MaDonHang = $"NHAP-{o.Id:D5}",
            NgayTao = o.NgayTao,
            TrangThai = o.TrangThai,
            NguoiDungId = o.NguoiDungId,
            TenNhanVien = o.NguoiDung?.HoTen ?? o.NguoiDung?.TenDangNhap,
            KhachHangId = o.KhachHangId,
            TenKhachHang = kh?.TenKhachHang,
            SoDienThoaiKhach = kh?.SoDienThoai,
            MaGiamGia = o.MaGiamGia?.MaCode,
            DiemSuDung = diemSuDung,
            TongTienHang = tongTienHang,
            SoTienGiam = soTienGiam,
            TongTienThanhToan = o.TongTienThanhToan,
            TongSoLuongMon = o.ChiTietDonHangs.Sum(ct => ct.SoLuong),
            Items = detailDtos
        };
    }
}
