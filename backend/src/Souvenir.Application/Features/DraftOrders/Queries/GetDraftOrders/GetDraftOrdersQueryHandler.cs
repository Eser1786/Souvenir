using MediatR;
using Microsoft.EntityFrameworkCore;
using Souvenir.Application.DTOs.Orders;
using Souvenir.Application.Interfaces;
using Souvenir.Domain.Entities;

namespace Souvenir.Application.Features.DraftOrders.Queries.GetDraftOrders;

public class GetDraftOrdersQueryHandler : IRequestHandler<GetDraftOrdersQuery, List<DraftOrderDto>>
{
    private readonly IApplicationDbContext _context;

    public GetDraftOrdersQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<DraftOrderDto>> Handle(GetDraftOrdersQuery request, CancellationToken cancellationToken)
    {
        var orders = await _context.DonHangs
            .AsNoTracking()
            .Where(o => o.TrangThai == TrangThaiDonHang.LuuNhap || o.TrangThai == "LuuNhap")
            .Include(o => o.ChiTietDonHangs)
            .Include(o => o.NguoiDung)
            .Include(o => o.MaGiamGia)
            .OrderByDescending(o => o.NgayTao)
            .ToListAsync(cancellationToken);

        if (orders.Count == 0) return new List<DraftOrderDto>();

        // Collect all product ids
        var productIds = orders.SelectMany(o => o.ChiTietDonHangs.Select(ct => ct.SanPhamId)).Distinct().ToList();
        var products = await _context.SanPhams
            .AsNoTracking()
            .Where(p => productIds.Contains(p.Id))
            .ToDictionaryAsync(p => p.Id, cancellationToken);

        // Collect all customer ids
        var customerIds = orders.Where(o => o.KhachHangId.HasValue).Select(o => o.KhachHangId!.Value).Distinct().ToList();
        var customers = customerIds.Count > 0
            ? await _context.KhachHangs.AsNoTracking().Where(k => customerIds.Contains(k.Id)).ToDictionaryAsync(k => k.Id, cancellationToken)
            : new Dictionary<int, KhachHang>();

        var result = new List<DraftOrderDto>();
        foreach (var o in orders)
        {
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

            KhachHang? kh = null;
            if (o.KhachHangId.HasValue) customers.TryGetValue(o.KhachHangId.Value, out kh);

            decimal soTienGiam = Math.Max(0m, tongTienHang - o.TongTienThanhToan);

            result.Add(new DraftOrderDto
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
                TongTienHang = tongTienHang,
                SoTienGiam = soTienGiam,
                TongTienThanhToan = o.TongTienThanhToan,
                TongSoLuongMon = o.ChiTietDonHangs.Sum(ct => ct.SoLuong),
                Items = detailDtos
            });
        }

        return result;
    }
}
