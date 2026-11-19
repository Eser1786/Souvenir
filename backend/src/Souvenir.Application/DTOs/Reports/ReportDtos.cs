using System;
using System.Collections.Generic;

namespace Souvenir.Application.DTOs.Reports;

/// <summary>
/// Thống kê tổng quan doanh thu và số liệu theo kỳ (UC14).
/// </summary>
public class RevenueSummaryDto
{
    public decimal TongDoanhThu { get; set; }
    public int TongDonHang { get; set; }
    public int TongSanPhamDaBan { get; set; }
    public decimal TongTienGiam { get; set; }
    public decimal TongLoiNhuanUocTinh { get; set; }
    public int TongTonKho { get; set; }
    public decimal TongGiaTriTonKho { get; set; }
    public int TongKhachHang { get; set; }
    public decimal DoanhThuTrungBinhMoiDon => TongDonHang > 0 ? Math.Round(TongDoanhThu / TongDonHang, 0) : 0m;
    public string KyBaoCao { get; set; } = string.Empty;
    public DateTime TuNgay { get; set; }
    public DateTime DenNgay { get; set; }
    public List<RevenueTimelinePointDto> Timeline { get; set; } = new();
}

public class RevenueTimelinePointDto
{
    public string Date { get; set; } = string.Empty;
    public string Label { get; set; } = string.Empty;
    public decimal DoanhThu { get; set; }
    public int SoDonHang { get; set; }
    public int SoLuongBan { get; set; }
}

/// <summary>
/// DTO Báo cáo Top sản phẩm bán chạy nhất (UC14).
/// </summary>
public class TopProductsReportDto
{
    public List<TopProductItemDto> Items { get; set; } = new();
    public decimal TongDoanhThuTop { get; set; }
    public int TongSoLuongTop { get; set; }
    public decimal TongDoanhThuToanPhan { get; set; }
    public DateTime TuNgay { get; set; }
    public DateTime DenNgay { get; set; }
}

public class TopProductItemDto
{
    public int SanPhamId { get; set; }
    public string MaSP { get; set; } = string.Empty;
    public string TenSP { get; set; } = string.Empty;
    public string DanhMuc { get; set; } = string.Empty;
    public string? HinhAnh { get; set; }
    public decimal GiaBan { get; set; }
    public decimal GiaNhap { get; set; }
    public int SoLuongDaBan { get; set; }
    public decimal TongDoanhThu { get; set; }
    public decimal LoiNhuanUocTinh { get; set; }
    public double PhanTramDoanhThu { get; set; }
}

/// <summary>
/// DTO Báo cáo theo ca làm việc (UC14).
/// </summary>
public class ShiftsReportDto
{
    public List<ShiftReportItemDto> Items { get; set; } = new();
    public decimal TongDoanhThuCacCa { get; set; }
    public decimal TongTienMatCacCa { get; set; }
    public decimal TongChuyenKhoanCacCa { get; set; }
    public int TongSoCa { get; set; }
    public DateTime TuNgay { get; set; }
    public DateTime DenNgay { get; set; }
}

public class ShiftReportItemDto
{
    public int CaLamViecId { get; set; }
    public int NhanVienId { get; set; }
    public string TenNhanVien { get; set; } = string.Empty;
    public DateTime ThoiGianMoCa { get; set; }
    public DateTime? ThoiGianChotCa { get; set; }
    public decimal TienMatBanGiao { get; set; }
    public decimal? TienMatThucTe { get; set; }
    public decimal TongDoanhThuTienMat { get; set; }
    public decimal TongDoanhThuChuyenKhoan { get; set; }
    public decimal TongDoanhThuCa { get; set; }
    public decimal ChenhLech { get; set; }
    public string TrangThai { get; set; } = string.Empty;
    public string? LyDoChenhLech { get; set; }
    public int SoDonHang { get; set; }
}
