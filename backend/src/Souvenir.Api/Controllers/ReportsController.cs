using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Souvenir.Application.DTOs.Reports;
using Souvenir.Application.Features.Reports.Queries.GetRevenueReport;
using Souvenir.Application.Features.Reports.Queries.GetShiftsReport;
using Souvenir.Application.Features.Reports.Queries.GetTopProducts;
using Souvenir.Domain.Constants;

namespace Souvenir.Api.Controllers;

/// <summary>
/// UC14 - Phân hệ Báo cáo Doanh thu & Thống kê dành cho Quản lý.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = $"{VaiTroConstants.QuanLy},Admin,QUAN_LY,Quản lý")]
public class ReportsController : ControllerBase
{
    private readonly ISender _sender;

    public ReportsController(ISender sender)
    {
        _sender = sender;
    }

    /// <summary>
    /// Thống kê tổng doanh thu, số đơn, lợi nhuận và biểu đồ theo ngày/tuần/tháng (UC14).
    /// </summary>
    [HttpGet("revenue")]
    [ProducesResponseType(typeof(RevenueSummaryDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetRevenueReport(
        [FromQuery] string? period,
        [FromQuery] DateTime? fromDate,
        [FromQuery] DateTime? toDate,
        CancellationToken cancellationToken)
    {
        var result = await _sender.Send(
            new GetRevenueReportQuery(period, fromDate, toDate),
            cancellationToken);

        return Ok(result);
    }

    /// <summary>
    /// Lấy danh sách Top sản phẩm bán chạy nhất kèm số lượng bán và doanh thu (UC14).
    /// </summary>
    [HttpGet("top-products")]
    [ProducesResponseType(typeof(TopProductsReportDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetTopProducts(
        [FromQuery] int limit = 5,
        [FromQuery] string? period = "thisMonth",
        [FromQuery] DateTime? fromDate = null,
        [FromQuery] DateTime? toDate = null,
        CancellationToken cancellationToken = default)
    {
        var result = await _sender.Send(
            new GetTopProductsQuery(limit, period, fromDate, toDate),
            cancellationToken);

        return Ok(result);
    }

    /// <summary>
    /// Thống kê doanh thu và đối soát chênh lệch theo từng ca làm việc (UC14).
    /// </summary>
    [HttpGet("shifts")]
    [ProducesResponseType(typeof(ShiftsReportDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetShiftsReport(
        [FromQuery] string? period = "thisMonth",
        [FromQuery] DateTime? fromDate = null,
        [FromQuery] DateTime? toDate = null,
        CancellationToken cancellationToken = default)
    {
        var result = await _sender.Send(
            new GetShiftsReportQuery(period, fromDate, toDate),
            cancellationToken);

        return Ok(result);
    }

    /// <summary>
    /// Xuất dữ liệu báo cáo ra file CSV chuẩn UTF-8 (mở trực tiếp bằng Microsoft Excel) (UC14).
    /// </summary>
    [HttpGet("export")]
    [Produces("text/csv")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> ExportReport(
        [FromQuery] string type = "revenue",
        [FromQuery] string? period = "thisMonth",
        [FromQuery] DateTime? fromDate = null,
        [FromQuery] DateTime? toDate = null,
        CancellationToken cancellationToken = default)
    {
        var csv = new StringBuilder();
        // UTF-8 BOM so Excel opens Vietnamese characters without garbled text
        var bom = Encoding.UTF8.GetPreamble();

        if (string.Equals(type, "top-products", StringComparison.OrdinalIgnoreCase))
        {
            var data = await _sender.Send(new GetTopProductsQuery(20, period, fromDate, toDate), cancellationToken);
            csv.AppendLine("Mã SP,Tên sản phẩm,Danh mục,Đơn giá bán,Đơn giá nhập,Số lượng đã bán,Tổng doanh thu (VNĐ),Lợi nhuận ước tính (VNĐ),Tỷ trọng (%)");

            foreach (var item in data.Items)
            {
                var safeTen = item.TenSP.Replace("\"", "\"\"");
                var safeDm = item.DanhMuc.Replace("\"", "\"\"");
                csv.AppendLine($"\"{item.MaSP}\",\"{safeTen}\",\"{safeDm}\",{item.GiaBan},{item.GiaNhap},{item.SoLuongDaBan},{item.TongDoanhThu},{item.LoiNhuanUocTinh},{item.PhanTramDoanhThu}");
            }

            var bytes = bom.Concat(Encoding.UTF8.GetBytes(csv.ToString())).ToArray();
            return File(bytes, "text/csv; charset=utf-8", $"BaoCao_TopSanPham_{DateTime.Now:yyyyMMdd_HHmmss}.csv");
        }
        else if (string.Equals(type, "shifts", StringComparison.OrdinalIgnoreCase))
        {
            var data = await _sender.Send(new GetShiftsReportQuery(period, fromDate, toDate), cancellationToken);
            csv.AppendLine("Mã ca,Nhân viên,Thời gian mở ca,Thời gian chốt ca,Tiền mặt bàn giao,Tiền mặt thực tế,Doanh thu tiền mặt,Doanh thu chuyển khoản,Tổng doanh thu ca,Chênh lệch,Trạng thái,Lý do chênh lệch");

            foreach (var s in data.Items)
            {
                var safeTen = s.TenNhanVien.Replace("\"", "\"\"");
                var safeLyDo = (s.LyDoChenhLech ?? string.Empty).Replace("\"", "\"\"");
                var moCa = s.ThoiGianMoCa.ToString("dd/MM/yyyy HH:mm");
                var chotCa = s.ThoiGianChotCa.HasValue ? s.ThoiGianChotCa.Value.ToString("dd/MM/yyyy HH:mm") : "Đang mở";
                csv.AppendLine($"\"{s.CaLamViecId}\",\"{safeTen}\",\"{moCa}\",\"{chotCa}\",{s.TienMatBanGiao},{s.TienMatThucTe ?? 0},{s.TongDoanhThuTienMat},{s.TongDoanhThuChuyenKhoan},{s.TongDoanhThuCa},{s.ChenhLech},\"{s.TrangThai}\",\"{safeLyDo}\"");
            }

            var bytes = bom.Concat(Encoding.UTF8.GetBytes(csv.ToString())).ToArray();
            return File(bytes, "text/csv; charset=utf-8", $"BaoCao_CaLamViec_{DateTime.Now:yyyyMMdd_HHmmss}.csv");
        }
        else
        {
            var data = await _sender.Send(new GetRevenueReportQuery(period, fromDate, toDate), cancellationToken);
            csv.AppendLine("BÁO CÁO DOANH THU & HOẠT ĐỘNG BÁN HÀNG CỬA HÀNG LƯU NIỆM");
            csv.AppendLine($"Kỳ báo cáo,{data.KyBaoCao}");
            csv.AppendLine($"Thời gian từ,{data.TuNgay:dd/MM/yyyy HH:mm},đến,{data.DenNgay:dd/MM/yyyy HH:mm}");
            csv.AppendLine($"Tổng doanh thu (VNĐ),{data.TongDoanhThu}");
            csv.AppendLine($"Tổng số đơn hàng,{data.TongDonHang}");
            csv.AppendLine($"Tổng sản phẩm bán ra,{data.TongSanPhamDaBan}");
            csv.AppendLine($"Tổng tiền giảm giá (VNĐ),{data.TongTienGiam}");
            csv.AppendLine($"Lợi nhuận gộp ước tính (VNĐ),{data.TongLoiNhuanUocTinh}");
            csv.AppendLine($"Tổng tồn kho hiện tại (Món),{data.TongTonKho}");
            csv.AppendLine($"Tổng giá trị tồn kho (VNĐ),{data.TongGiaTriTonKho}");
            csv.AppendLine();
            csv.AppendLine("CHI TIẾT THEO MỐC THỜI GIAN:");
            csv.AppendLine("Thời gian,Mốc hiển thị,Doanh thu (VNĐ),Số đơn hàng,Số lượng sản phẩm bán");

            foreach (var p in data.Timeline)
            {
                csv.AppendLine($"\"{p.Date}\",\"{p.Label}\",{p.DoanhThu},{p.SoDonHang},{p.SoLuongBan}");
            }

            var bytes = bom.Concat(Encoding.UTF8.GetBytes(csv.ToString())).ToArray();
            return File(bytes, "text/csv; charset=utf-8", $"BaoCao_DoanhThu_{DateTime.Now:yyyyMMdd_HHmmss}.csv");
        }
    }
}
