using System.Globalization;
using System.Text.RegularExpressions;
using Microsoft.EntityFrameworkCore;
using Souvenir.Application.DTOs.Returns;
using Souvenir.Application.Features.Orders;
using Souvenir.Application.Interfaces;
using Souvenir.Domain.Entities;

namespace Souvenir.Application.Features.Returns;

/// <summary>Quy tắc đổi trả dùng chung cho tra cứu và lập phiếu trả (UC08).</summary>
public static partial class ReturnPolicy
{
    public const int SoNgayDoiTraMacDinh = 7;

    [GeneratedRegex(@"^DH\d{6}-0*(\d+)$", RegexOptions.IgnoreCase)]
    private static partial Regex MaDonHangRegex();

    /// <summary>Đọc "DH261117-00012", "12" hoặc "#12" thành Id đơn.</summary>
    public static int? ParseMaDonHang(string? value)
    {
        if (string.IsNullOrWhiteSpace(value)) return null;
        var v = value.Trim().TrimStart('#');
        var m = MaDonHangRegex().Match(v);
        if (m.Success && int.TryParse(m.Groups[1].Value, out var fromCode)) return fromCode;
        return int.TryParse(v, out var id) && id > 0 ? id : null;
    }

    public static async Task<int> GetSoNgayDoiTraAsync(IApplicationDbContext context, CancellationToken ct)
    {
        var param = await context.ThamSos.AsNoTracking()
            .FirstOrDefaultAsync(t => t.TenThamSo == "SoNgayChoPhepDoiTra", ct);
        return param != null && int.TryParse(param.GiaTri, NumberStyles.Integer, CultureInfo.InvariantCulture, out var days) && days >= 0
            ? days
            : SoNgayDoiTraMacDinh;
    }

    public static async Task<decimal> GetTyLeTichDiemAsync(IApplicationDbContext context, CancellationToken ct)
    {
        var param = await context.ThamSos.AsNoTracking()
            .FirstOrDefaultAsync(t => t.TenThamSo == "TyLeTichDiem", ct);
        return param != null && decimal.TryParse(param.GiaTri, NumberStyles.Any, CultureInfo.InvariantCulture, out var rate) && rate >= 0
            ? rate
            : 0.01m;
    }

    /// <summary>Hạn cuối được đổi trả (hết ngày thứ N kể từ ngày mua).</summary>
    public static DateTime TinhHanDoiTra(DateTime ngayMua, int soNgay) => ngayMua.Date.AddDays(soNgay + 1).AddTicks(-1);

    /// <summary>Tỷ lệ tiền khách thực trả / tổng tiền hàng, để phân bổ giảm giá vào đơn giá hoàn.</summary>
    public static decimal TyLeThanhToan(DonHang donHang)
    {
        var tongTienHang = donHang.TinhTongTienHang();
        return tongTienHang <= 0 ? 0 : donHang.TongTienThanhToan / tongTienHang;
    }

    public static decimal TinhDonGiaHoan(decimal donGia, decimal tyLe) => Math.Round(donGia * tyLe, 0, MidpointRounding.AwayFromZero);

    /// <summary>Số lượng đã trả của từng sản phẩm trong các đơn.</summary>
    public static async Task<Dictionary<(int DonHangId, int SanPhamId), int>> GetSoLuongDaTraAsync(
        IApplicationDbContext context, IReadOnlyCollection<int> donHangIds, CancellationToken ct)
    {
        var rows = await context.ChiTietTraHangs.AsNoTracking()
            .Where(ct2 => donHangIds.Contains(ct2.PhieuTraHang!.DonHangGocId))
            .GroupBy(ct2 => new { ct2.PhieuTraHang!.DonHangGocId, ct2.SanPhamId })
            .Select(g => new { g.Key.DonHangGocId, g.Key.SanPhamId, SoLuong = g.Sum(x => x.SoLuongTra) })
            .ToListAsync(ct);
        return rows.ToDictionary(r => (r.DonHangGocId, r.SanPhamId), r => r.SoLuong);
    }

    public static async Task<Dictionary<int, decimal>> GetTongDaHoanAsync(
        IApplicationDbContext context, IReadOnlyCollection<int> donHangIds, CancellationToken ct)
    {
        var rows = await context.PhieuTraHangs.AsNoTracking()
            .Where(p => donHangIds.Contains(p.DonHangGocId))
            .GroupBy(p => p.DonHangGocId)
            .Select(g => new { DonHangId = g.Key, Tong = g.Sum(x => x.TongTienHoan) })
            .ToListAsync(ct);
        return rows.ToDictionary(r => r.DonHangId, r => r.Tong);
    }

    public static ReturnableOrderDto ToReturnableDto(
        DonHang donHang,
        KhachHang? khachHang,
        int soNgayDoiTra,
        DateTime now,
        IReadOnlyDictionary<(int, int), int> daTra,
        decimal tongDaHoan)
    {
        var tyLe = TyLeThanhToan(donHang);
        var hanDoiTra = TinhHanDoiTra(donHang.NgayTao, soNgayDoiTra);
        var conHan = now <= hanDoiTra;

        var items = donHang.ChiTietDonHangs
            .OrderBy(ct => ct.SanPhamId)
            .Select(ct =>
            {
                var soDaTra = daTra.TryGetValue((donHang.Id, ct.SanPhamId), out var n) ? n : 0;
                return new ReturnableItemDto
                {
                    SanPhamId = ct.SanPhamId,
                    MaSP = ct.SanPham?.MaSP ?? string.Empty,
                    TenSP = ct.SanPham?.TenSP ?? string.Empty,
                    HinhAnh = ct.SanPham?.HinhAnh,
                    SoLuongMua = ct.SoLuong,
                    SoLuongDaTra = soDaTra,
                    SoLuongConTra = Math.Max(0, ct.SoLuong - soDaTra),
                    DonGia = ct.DonGia,
                    DonGiaHoan = TinhDonGiaHoan(ct.DonGia, tyLe)
                };
            })
            .ToList();

        string? lyDo = null;
        if (donHang.TrangThai != TrangThaiDonHang.DaThanhToan && donHang.TrangThai != TrangThaiDonHang.DaTraHang)
            lyDo = "Đơn hàng chưa thanh toán nên không thể trả hàng.";
        else if (!conHan)
            lyDo = $"Đơn đã quá hạn đổi trả {soNgayDoiTra} ngày (hạn cuối {hanDoiTra:dd/MM/yyyy}).";
        else if (items.All(i => i.SoLuongConTra == 0))
            lyDo = "Tất cả sản phẩm của đơn đã được trả.";

        return new ReturnableOrderDto
        {
            Id = donHang.Id,
            MaDonHang = OrderMapper.FormatMaDonHang(donHang),
            NgayTao = donHang.NgayTao,
            TrangThai = donHang.TrangThai,
            KhachHangId = donHang.KhachHangId,
            TenKhachHang = khachHang?.TenKhachHang,
            SoDienThoaiKhach = khachHang?.SoDienThoai,
            TenHinhThucThanhToan = donHang.HinhThucThanhToan?.TenHinhThuc ?? string.Empty,
            MaGiamGia = donHang.MaGiamGia?.MaCode,
            TongTienHang = donHang.TinhTongTienHang(),
            TongTienThanhToan = donHang.TongTienThanhToan,
            TongDaHoan = tongDaHoan,
            SoNgayChoPhepDoiTra = soNgayDoiTra,
            HanDoiTra = hanDoiTra,
            ConHanDoiTra = conHan,
            CoTheTra = lyDo == null,
            LyDoKhongTheTra = lyDo,
            Items = items
        };
    }

    /// <summary>Giờ hiện tại theo cùng quy ước với DonHang.NgayTao (giờ địa phương, Kind = Unspecified).</summary>
    public static DateTime Now() => DateTime.UtcNow;
}
