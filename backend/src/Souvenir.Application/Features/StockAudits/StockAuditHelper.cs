using Microsoft.EntityFrameworkCore;
using Souvenir.Application.DTOs.StockAudits;
using Souvenir.Application.Interfaces;
using Souvenir.Domain.Entities;

namespace Souvenir.Application.Features.StockAudits;

/// <summary>Xử lý nghiệp vụ dùng chung cho Lưu tạm (DangKiemKe) và Cân bằng tồn kho (DaCanBang) của UC13.</summary>
public static class StockAuditHelper
{
    private const string InMemoryProvider = "Microsoft.EntityFrameworkCore.InMemory";

    public static DateTime NowUnspecified() =>
        DateTime.SpecifyKind(DateTime.Now, DateTimeKind.Unspecified);

    public static IQueryable<PhieuKiemKe> QueryWithDetails(IApplicationDbContext context) =>
        context.PhieuKiemKes
            .AsNoTracking()
            .Include(p => p.NguoiDung)
            .Include(p => p.ChiTietKiemKes)
                .ThenInclude(c => c.SanPham)
                    .ThenInclude(s => s!.DanhMuc)
            .Include(p => p.ChiTietKiemKes)
                .ThenInclude(c => c.SanPham)
                    .ThenInclude(s => s!.DonViTinh);

    public static StockAuditDto ToDto(PhieuKiemKe phieu)
    {
        var items = phieu.ChiTietKiemKes
            .OrderBy(c => c.SanPhamId)
            .Select(c => new StockAuditDetailDto
            {
                SanPhamId = c.SanPhamId,
                MaSP = c.SanPham?.MaSP ?? string.Empty,
                TenSP = c.SanPham?.TenSP ?? string.Empty,
                TenDanhMuc = c.SanPham?.DanhMuc?.TenDanhMuc ?? string.Empty,
                TenDonVi = c.SanPham?.DonViTinh?.TenDonVi,
                SoTonSoSach = c.SoTonSoSach,
                SoTonThucTe = c.SoTonThucTe,
                SoLuongChenhLech = c.SoLuongChenhLech,
                LyDoChenhLech = c.LyDoChenhLech
            })
            .ToList();

        return new StockAuditDto
        {
            Id = phieu.Id,
            MaPhieu = phieu.MaPhieu,
            NguoiDungId = phieu.NguoiDungId,
            TenNguoiKiemKe = phieu.NguoiDung?.HoTen ?? string.Empty,
            NgayKiemKe = phieu.NgayKiemKe,
            TrangThai = phieu.TrangThai,
            TongSanPham = items.Count,
            SoSanPhamChenhLech = items.Count(i => i.SoLuongChenhLech != 0),
            TongChenhLechThua = items.Where(i => i.SoLuongChenhLech > 0).Sum(i => i.SoLuongChenhLech),
            TongChenhLechThieu = items.Where(i => i.SoLuongChenhLech < 0).Sum(i => Math.Abs(i.SoLuongChenhLech)),
            Items = items
        };
    }

    public static async Task<StockAuditResult<StockAuditDto>> SaveOrBalanceAsync(
        IApplicationDbContext context,
        int nguoiDungId,
        SaveStockAuditRequest? request,
        bool isBalance,
        CancellationToken ct)
    {
        if (request == null || request.Items == null || request.Items.Count == 0)
        {
            return StockAuditResult<StockAuditDto>.Failed("Danh sách sản phẩm kiểm kê không được để trống.");
        }

        if (request.Items.Any(i => i.SoTonThucTe < 0))
        {
            return StockAuditResult<StockAuditDto>.Failed("Số lượng tồn thực tế không được nhỏ hơn 0.");
        }

        if (request.Items.GroupBy(i => i.SanPhamId).Any(g => g.Count() > 1))
        {
            return StockAuditResult<StockAuditDto>.Failed("Mỗi sản phẩm chỉ được xuất hiện một lần trong phiếu kiểm kê.");
        }

        if (request.Items.Any(i => (i.LyDoChenhLech?.Trim().Length ?? 0) > 255))
        {
            return StockAuditResult<StockAuditDto>.Failed("Lý do chênh lệch tối đa 255 ký tự.");
        }

        var productIds = request.Items.Select(i => i.SanPhamId).ToList();
        var products = await context.SanPhams
            .Include(p => p.DanhMuc)
            .Include(p => p.DonViTinh)
            .Where(p => productIds.Contains(p.Id))
            .ToDictionaryAsync(p => p.Id, ct);

        var missingIds = productIds.Where(id => !products.ContainsKey(id)).ToList();
        if (missingIds.Count > 0)
        {
            return StockAuditResult<StockAuditDto>.Failed(
                $"Không tìm thấy sản phẩm có mã: {string.Join(", ", missingIds)}.", 404);
        }

        PhieuKiemKe? existing = null;
        if (request.Id.HasValue)
        {
            existing = await context.PhieuKiemKes
                .Include(p => p.ChiTietKiemKes)
                .FirstOrDefaultAsync(p => p.Id == request.Id.Value, ct);

            if (existing == null)
            {
                return StockAuditResult<StockAuditDto>.Failed(
                    $"Không tìm thấy phiếu kiểm kê #{request.Id.Value}.", 404);
            }

            if (existing.TrangThai == TrangThaiPhieuKiemKe.DaCanBang)
            {
                return StockAuditResult<StockAuditDto>.Failed(
                    "Phiếu kiểm kê đã cân bằng tồn kho, không thể chỉnh sửa.", 409);
            }
        }

        // Tính chênh lệch và kiểm tra ràng buộc bắt buộc nhập lý do khi cân bằng (UC13)
        var missingReasonProducts = new List<string>();
        foreach (var item in request.Items)
        {
            var sp = products[item.SanPhamId];
            var diff = item.SoTonThucTe - sp.SoLuongTon;
            if (isBalance && diff != 0 && string.IsNullOrWhiteSpace(item.LyDoChenhLech))
            {
                missingReasonProducts.Add($"{sp.MaSP} - {sp.TenSP}");
            }
        }

        if (missingReasonProducts.Count > 0)
        {
            return StockAuditResult<StockAuditDto>.Failed(
                $"Bắt buộc nhập lý do giải trình chênh lệch cho sản phẩm: {string.Join("; ", missingReasonProducts)}.");
        }

        var now = NowUnspecified();
        var isNew = existing == null;
        var targetStatus = isBalance ? TrangThaiPhieuKiemKe.DaCanBang : TrangThaiPhieuKiemKe.DangKiemKe;

        var isRelational = !string.Equals(context.Database.ProviderName, InMemoryProvider, StringComparison.Ordinal);
        await using var transaction = isRelational
            ? await context.Database.BeginTransactionAsync(ct)
            : null;

        try
        {
            PhieuKiemKe phieu;
            if (existing == null)
            {
                phieu = new PhieuKiemKe
                {
                    MaPhieu = $"PKK{now:yyMMddHHmmss}{Random.Shared.Next(10, 100)}",
                    NguoiDungId = nguoiDungId,
                    NgayKiemKe = now,
                    TrangThai = targetStatus
                };
                context.PhieuKiemKes.Add(phieu);
            }
            else
            {
                phieu = existing;
                phieu.NguoiDungId = nguoiDungId;
                phieu.NgayKiemKe = now;
                phieu.TrangThai = targetStatus;
            }

            var existingDetails = phieu.ChiTietKiemKes.ToDictionary(d => d.SanPhamId);
            var incomingIds = productIds.ToHashSet();

            var toRemove = phieu.ChiTietKiemKes.Where(d => !incomingIds.Contains(d.SanPhamId)).ToList();
            foreach (var obsolete in toRemove)
            {
                phieu.ChiTietKiemKes.Remove(obsolete);
                context.ChiTietKiemKes.Remove(obsolete);
            }

            foreach (var item in request.Items)
            {
                var sp = products[item.SanPhamId];
                var soTonSoSach = sp.SoLuongTon;
                var trimmedReason = string.IsNullOrWhiteSpace(item.LyDoChenhLech)
                    ? null
                    : item.LyDoChenhLech.Trim();

                if (existingDetails.TryGetValue(item.SanPhamId, out var detail))
                {
                    detail.SoTonSoSach = soTonSoSach;
                    detail.SoTonThucTe = item.SoTonThucTe;
                    detail.TinhChenhLech();
                    detail.LyDoChenhLech = trimmedReason;
                }
                else
                {
                    var newDetail = new ChiTietKiemKe
                    {
                        SanPhamId = item.SanPhamId,
                        SoTonSoSach = soTonSoSach,
                        SoTonThucTe = item.SoTonThucTe,
                        LyDoChenhLech = trimmedReason
                    };
                    newDetail.TinhChenhLech();
                    phieu.ChiTietKiemKes.Add(newDetail);
                }

                if (isBalance)
                {
                    sp.SoLuongTon = item.SoTonThucTe;
                }
            }

            await context.SaveChangesAsync(ct);
            if (transaction != null)
            {
                await transaction.CommitAsync(ct);
            }

            var saved = await QueryWithDetails(context).FirstAsync(p => p.Id == phieu.Id, ct);
            var dto = ToDto(saved);

            return isNew
                ? StockAuditResult<StockAuditDto>.Created(
                    dto,
                    isBalance ? "Cân bằng tồn kho thành công." : "Đã lưu tạm phiếu kiểm kê.")
                : StockAuditResult<StockAuditDto>.Success(
                    dto,
                    isBalance ? "Cân bằng tồn kho thành công." : "Đã cập nhật phiếu kiểm kê tạm.");
        }
        catch
        {
            if (transaction != null)
            {
                await transaction.RollbackAsync(ct);
            }
            throw;
        }
    }
}
