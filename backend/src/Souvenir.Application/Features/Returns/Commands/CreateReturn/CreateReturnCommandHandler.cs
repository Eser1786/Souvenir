using System.Globalization;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Souvenir.Application.DTOs.Returns;
using Souvenir.Application.Features.Orders;
using Souvenir.Application.Interfaces;
using Souvenir.Domain.Entities;

namespace Souvenir.Application.Features.Returns.Commands.CreateReturn;

/// <summary>
/// UC08 - Trả hàng / Hoàn tiền. Trong MỘT transaction:
///   1. Khóa đơn gốc và chuyển trạng thái sang DaTraHang
///   2. Kiểm tra số lượng trả ≤ số lượng mua − số lượng đã trả ở các phiếu trước
///   3. Lưu PHIEU_TRA_HANG + CHI_TIET_TRA_HANG
///   4. Cộng trả SAN_PHAM.soLuongTon
///   5. Trừ lại điểm tích lũy khách đã được cộng cho phần tiền hoàn (UC10)
/// Lỗi ở bất kỳ bước nào thì rollback: không có phiếu, đơn và tồn kho giữ nguyên.
/// </summary>
public class CreateReturnCommandHandler : IRequestHandler<CreateReturnCommand, ReturnOperationResult<ReturnDto>>
{
    private static readonly CultureInfo ViVn = new("vi-VN");
    private readonly IApplicationDbContext _context;

    public CreateReturnCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<ReturnOperationResult<ReturnDto>> Handle(CreateReturnCommand command, CancellationToken cancellationToken)
    {
        var req = command.Request;
        if (req == null || req.Items == null || req.Items.Count == 0)
        {
            return Fail("Vui lòng chọn ít nhất một sản phẩm cần trả.");
        }

        var lyDo = req.LyDoTra?.Trim() ?? string.Empty;
        if (lyDo.Length == 0) return Fail("Vui lòng nhập lý do trả hàng.");
        if (lyDo.Length > 255) return Fail("Lý do trả hàng tối đa 255 ký tự.");

        if (req.Items.Any(i => i.SoLuongTra <= 0))
        {
            return Fail("Số lượng trả của mỗi sản phẩm phải lớn hơn 0.");
        }

        var invalidStatus = req.Items.FirstOrDefault(i => !TinhTrangHangTra.HopLe.Contains(i.TinhTrangHang));
        if (invalidStatus != null)
        {
            return Fail($"Tình trạng hàng '{invalidStatus.TinhTrangHang}' không hợp lệ. Dùng NguyenVen, LoiNhaSanXuat hoặc HuHongDoKhach.");
        }

        // Luồng 3a: hàng hư hỏng do lỗi của khách → từ chối đổi trả
        if (req.Items.Any(i => i.TinhTrangHang == TinhTrangHangTra.HuHongDoKhach))
        {
            return Fail("Có sản phẩm bị hư hỏng do khách, không đủ điều kiện đổi trả theo chính sách.", 422);
        }

        if (req.Items.GroupBy(i => i.SanPhamId).Any(g => g.Count() > 1))
        {
            return Fail("Mỗi sản phẩm chỉ được khai báo một dòng trên phiếu trả.");
        }

        var donHangId = req.DonHangId ?? ReturnPolicy.ParseMaDonHang(req.MaDonHang);
        if (donHangId == null)
        {
            return Fail("Vui lòng cung cấp Id hoặc mã đơn hàng gốc.");
        }

        var donHang = await _context.DonHangs.AsNoTracking()
            .Include(d => d.HinhThucThanhToan)
            .Include(d => d.ChiTietDonHangs).ThenInclude(ct => ct.SanPham)
            .FirstOrDefaultAsync(d => d.Id == donHangId.Value, cancellationToken);
        if (donHang == null)
        {
            return Fail($"Không tìm thấy hóa đơn gốc #{donHangId}.", 404);
        }

        if (donHang.TrangThai != TrangThaiDonHang.DaThanhToan && donHang.TrangThai != TrangThaiDonHang.DaTraHang)
        {
            return Fail("Hóa đơn gốc chưa ở trạng thái Đã thanh toán, không thể trả hàng.", 409);
        }

        var soNgay = await ReturnPolicy.GetSoNgayDoiTraAsync(_context, cancellationToken);
        var hanDoiTra = ReturnPolicy.TinhHanDoiTra(donHang.NgayTao, soNgay);
        var now = ReturnPolicy.Now();
        if (now > hanDoiTra)
        {
            return Fail($"Hóa đơn đã quá hạn đổi trả {soNgay} ngày (hạn cuối {hanDoiTra:dd/MM/yyyy}).", 422);
        }

        var chiTietGoc = donHang.ChiTietDonHangs.ToDictionary(ct => ct.SanPhamId);
        var notInOrder = req.Items.Where(i => !chiTietGoc.ContainsKey(i.SanPhamId)).Select(i => i.SanPhamId).ToList();
        if (notInOrder.Count > 0)
        {
            return Fail($"Sản phẩm Id {string.Join(", ", notInOrder)} không có trong hóa đơn gốc.");
        }

        var tyLe = ReturnPolicy.TyLeThanhToan(donHang);
        var tyLeTichDiem = await ReturnPolicy.GetTyLeTichDiemAsync(_context, cancellationToken);

        var phieu = new PhieuTraHang
        {
            MaPhieu = $"PT{now:yyMMddHHmmss}{Random.Shared.Next(10, 100)}",
            DonHangGocId = donHang.Id,
            NguoiDungId = command.NguoiDungId,
            LyDoTra = lyDo,
            NgayTra = now
        };

        int diemBiTru = 0;
        var tonMoi = new Dictionary<int, int>();

        await using (var transaction = await _context.Database.BeginTransactionAsync(cancellationToken))
        {
            try
            {
                // 1. Khóa dòng đơn gốc (UPDATE giữ row lock đến khi commit) + chuyển trạng thái DaTraHang
                var locked = await _context.DonHangs
                    .Where(d => d.Id == donHang.Id &&
                                (d.TrangThai == TrangThaiDonHang.DaThanhToan || d.TrangThai == TrangThaiDonHang.DaTraHang))
                    .ExecuteUpdateAsync(s => s.SetProperty(d => d.TrangThai, TrangThaiDonHang.DaTraHang), cancellationToken);
                if (locked == 0)
                {
                    await transaction.RollbackAsync(cancellationToken);
                    return Fail("Trạng thái hóa đơn gốc vừa thay đổi, vui lòng tra cứu lại.", 409);
                }

                // 2. Đọc lại số lượng đã trả sau khi khóa (chống 2 quầy trả cùng lúc)
                var daTra = await ReturnPolicy.GetSoLuongDaTraAsync(_context, new[] { donHang.Id }, cancellationToken);
                var daHoan = await ReturnPolicy.GetTongDaHoanAsync(_context, new[] { donHang.Id }, cancellationToken);

                foreach (var item in req.Items)
                {
                    var goc = chiTietGoc[item.SanPhamId];
                    var soDaTra = daTra.TryGetValue((donHang.Id, item.SanPhamId), out var n) ? n : 0;
                    var conTra = goc.SoLuong - soDaTra;
                    if (item.SoLuongTra > conTra)
                    {
                        await transaction.RollbackAsync(cancellationToken);
                        return Fail($"Sản phẩm '{goc.SanPham?.TenSP}' chỉ còn được trả {conTra} (đã mua {goc.SoLuong}, đã trả {soDaTra}).");
                    }

                    phieu.ChiTietTraHangs.Add(new ChiTietTraHang
                    {
                        SanPhamId = item.SanPhamId,
                        SoLuongTra = item.SoLuongTra,
                        DonGiaHoan = ReturnPolicy.TinhDonGiaHoan(goc.DonGia, tyLe),
                        TinhTrangHang = item.TinhTrangHang
                    });
                }

                // Tiền hoàn theo đơn giá đã phân bổ giảm giá, không vượt quá phần khách đã trả còn lại
                var tongHoan = phieu.ChiTietTraHangs.Sum(ct => ct.DonGiaHoan * ct.SoLuongTra);
                var conLaiCoTheHoan = Math.Max(0, donHang.TongTienThanhToan - (daHoan.TryGetValue(donHang.Id, out var h) ? h : 0));
                phieu.TongTienHoan = Math.Min(tongHoan, conLaiCoTheHoan);

                // 3. Lưu phiếu trả + chi tiết
                _context.PhieuTraHangs.Add(phieu);
                await _context.SaveChangesAsync(cancellationToken);

                // 4. Cộng trả tồn kho
                foreach (var ct in phieu.ChiTietTraHangs)
                {
                    var qty = ct.SoLuongTra;
                    await _context.SanPhams
                        .Where(p => p.Id == ct.SanPhamId)
                        .ExecuteUpdateAsync(s => s.SetProperty(p => p.SoLuongTon, p => p.SoLuongTon + qty), cancellationToken);
                }

                // 5. Trừ lại điểm đã tích cho phần tiền được hoàn (không để âm)
                if (donHang.KhachHangId.HasValue && phieu.TongTienHoan > 0)
                {
                    diemBiTru = (int)Math.Floor(phieu.TongTienHoan * tyLeTichDiem / 100m);
                    if (diemBiTru > 0)
                    {
                        var tru = diemBiTru;
                        await _context.KhachHangs
                            .Where(k => k.Id == donHang.KhachHangId.Value)
                            .ExecuteUpdateAsync(s => s.SetProperty(k => k.DiemTichLuy,
                                k => k.DiemTichLuy >= tru ? k.DiemTichLuy - tru : 0), cancellationToken);
                    }
                }

                await transaction.CommitAsync(cancellationToken);
            }
            catch
            {
                await transaction.RollbackAsync(CancellationToken.None);
                throw;
            }
        }

        var sanPhamIds = phieu.ChiTietTraHangs.Select(ct => ct.SanPhamId).ToList();
        tonMoi = await _context.SanPhams.AsNoTracking()
            .Where(p => sanPhamIds.Contains(p.Id))
            .ToDictionaryAsync(p => p.Id, p => p.SoLuongTon, cancellationToken);

        var tenNguoiLap = await _context.NguoiDungs.AsNoTracking()
            .Where(u => u.Id == command.NguoiDungId)
            .Select(u => u.HoTen)
            .FirstOrDefaultAsync(cancellationToken);

        var dto = new ReturnDto
        {
            Id = phieu.Id,
            MaPhieu = phieu.MaPhieu,
            DonHangGocId = donHang.Id,
            MaDonHangGoc = OrderMapper.FormatMaDonHang(donHang),
            NgayTra = phieu.NgayTra,
            NguoiDungId = phieu.NguoiDungId,
            TenNguoiLap = tenNguoiLap,
            LyDoTra = phieu.LyDoTra,
            TongTienHoan = phieu.TongTienHoan,
            HinhThucHoan = donHang.HinhThucThanhToan?.TenHinhThuc ?? string.Empty,
            TrangThaiDonGoc = TrangThaiDonHang.DaTraHang,
            DiemTichLuyBiTru = diemBiTru,
            Items = phieu.ChiTietTraHangs.Select(ct => new ReturnItemDto
            {
                SanPhamId = ct.SanPhamId,
                MaSP = chiTietGoc[ct.SanPhamId].SanPham?.MaSP ?? string.Empty,
                TenSP = chiTietGoc[ct.SanPhamId].SanPham?.TenSP ?? string.Empty,
                SoLuongTra = ct.SoLuongTra,
                DonGiaHoan = ct.DonGiaHoan,
                ThanhTien = ct.DonGiaHoan * ct.SoLuongTra,
                TinhTrangHang = ct.TinhTrangHang,
                SoLuongTonMoi = tonMoi.TryGetValue(ct.SanPhamId, out var t) ? t : null
            }).ToList()
        };

        return ReturnOperationResult<ReturnDto>.Success(dto, 201,
            string.Format(ViVn, "Đã lập phiếu trả {0}, hoàn {1:N0} đ cho khách.", phieu.MaPhieu, phieu.TongTienHoan));
    }

    private static ReturnOperationResult<ReturnDto> Fail(string message, int statusCode = 400) =>
        ReturnOperationResult<ReturnDto>.Failure(message, statusCode);
}
