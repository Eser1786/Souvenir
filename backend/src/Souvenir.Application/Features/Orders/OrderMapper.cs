using Souvenir.Application.DTOs.Orders;
using Souvenir.Domain.Entities;

namespace Souvenir.Application.Features.Orders;

public static class OrderMapper
{
    public static string FormatMaDonHang(DonHang donHang) => $"DH{donHang.NgayTao:yyMMdd}-{donHang.Id:D5}";

    public static OrderDto ToDto(DonHang donHang, decimal? tienKhachDua = null, IReadOnlyDictionary<int, int>? tonConLai = null)
    {
        var tongTienHang = donHang.TinhTongTienHang();
        return new OrderDto
        {
            Id = donHang.Id,
            MaDonHang = FormatMaDonHang(donHang),
            NgayTao = donHang.NgayTao,
            TrangThai = donHang.TrangThai,
            NguoiDungId = donHang.NguoiDungId,
            CaLamViecId = donHang.CaLamViecId,
            KhachHangId = donHang.KhachHangId,
            HinhThucThanhToanId = donHang.HinhThucThanhToanId,
            TenHinhThucThanhToan = donHang.HinhThucThanhToan?.TenHinhThuc ?? string.Empty,
            MaGiamGia = donHang.MaGiamGia?.MaCode,
            TongTienHang = tongTienHang,
            SoTienGiam = Math.Max(0, tongTienHang - donHang.TongTienThanhToan),
            TongTienThanhToan = donHang.TongTienThanhToan,
            TienKhachDua = tienKhachDua,
            TienThoi = tienKhachDua.HasValue ? tienKhachDua.Value - donHang.TongTienThanhToan : null,
            Items = donHang.ChiTietDonHangs.Select(ct => new OrderItemDto
            {
                SanPhamId = ct.SanPhamId,
                MaSP = ct.SanPham?.MaSP ?? string.Empty,
                TenSP = ct.SanPham?.TenSP ?? string.Empty,
                SoLuong = ct.SoLuong,
                DonGia = ct.DonGia,
                ThanhTien = ct.ThanhTien,
                SoLuongTonConLai = tonConLai != null && tonConLai.TryGetValue(ct.SanPhamId, out var ton)
                    ? ton
                    : ct.SanPham?.SoLuongTon ?? 0
            }).ToList()
        };
    }
}
