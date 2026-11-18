using MediatR;
using Microsoft.EntityFrameworkCore;
using Souvenir.Application.DTOs.Orders;
using Souvenir.Application.Interfaces;
using Souvenir.Domain.Entities;

namespace Souvenir.Application.Features.Orders.Commands.CreateOrder;

/// <summary>
/// UC07 - Thanh toán. Trong MỘT transaction:
///   1. Lưu DON_HANG
///   2. Lưu CHI_TIET_DON_HANG
///   3. Trừ SAN_PHAM.soLuongTon (câu UPDATE có điều kiện "soLuongTon >= soLuong" để chống bán âm kho khi 2 quầy thanh toán cùng lúc)
/// Bất kỳ bước nào lỗi thì rollback toàn bộ: không có đơn, không trừ kho.
/// </summary>
public class CreateOrderCommandHandler : IRequestHandler<CreateOrderCommand, OrderOperationResult<OrderDto>>
{
    private static readonly System.Globalization.CultureInfo ViVn = new("vi-VN");
    private readonly IApplicationDbContext _context;
    private readonly ICaLamViecLookup _caLamViecLookup;

    public CreateOrderCommandHandler(IApplicationDbContext context, ICaLamViecLookup caLamViecLookup)
    {
        _context = context;
        _caLamViecLookup = caLamViecLookup;
    }

    public async Task<OrderOperationResult<OrderDto>> Handle(CreateOrderCommand command, CancellationToken cancellationToken)
    {
        var req = command.Request;
        if (req == null || req.Items == null || req.Items.Count == 0)
        {
            return Fail("Giỏ hàng trống, không thể tạo đơn hàng.");
        }

        if (req.Items.Any(i => i.SoLuong <= 0))
        {
            return Fail("Số lượng mỗi sản phẩm phải lớn hơn 0.");
        }

        // Gộp các dòng trùng sản phẩm (bảng chi tiết có khóa chính kép donHangId + sanPhamId)
        var items = req.Items
            .GroupBy(i => i.SanPhamId)
            .Select(g => new { SanPhamId = g.Key, SoLuong = g.Sum(x => x.SoLuong) })
            .ToList();

        // ---- Hình thức thanh toán ----
        var methods = await _context.HinhThucThanhToans.AsNoTracking().ToListAsync(cancellationToken);
        HinhThucThanhToan? method = req.HinhThucThanhToanId.HasValue
            ? methods.FirstOrDefault(m => m.Id == req.HinhThucThanhToanId.Value)
            : string.IsNullOrWhiteSpace(req.PhuongThuc) ? null : PaymentMethodMatcher.Find(methods, req.PhuongThuc);

        if (method == null)
        {
            return Fail("Hình thức thanh toán không hợp lệ. Dùng PhuongThuc = \"TienMat\" hoặc \"ChuyenKhoan\".");
        }

        // ---- Ca làm việc (UC03): lấy ca truyền vào hoặc ca đang mở của nhân viên ----
        int caLamViecId;
        if (req.CaLamViecId.HasValue)
        {
            if (!await _caLamViecLookup.IsShiftOpenAsync(req.CaLamViecId.Value, cancellationToken))
            {
                return Fail($"Ca làm việc #{req.CaLamViecId} không tồn tại hoặc đã chốt.");
            }
            caLamViecId = req.CaLamViecId.Value;
        }
        else
        {
            var openShiftId = await _caLamViecLookup.GetOpenShiftIdAsync(command.NguoiDungId, cancellationToken);
            if (openShiftId == null)
            {
                return Fail("Nhân viên chưa mở ca làm việc. Vui lòng mở ca trước khi thanh toán.", 409);
            }
            caLamViecId = openShiftId.Value;
        }

        // ---- Sản phẩm & tồn kho ----
        var productIds = items.Select(i => i.SanPhamId).ToList();
        var products = await _context.SanPhams
            .AsNoTracking()
            .Where(p => productIds.Contains(p.Id))
            .ToDictionaryAsync(p => p.Id, cancellationToken);

        var missing = productIds.Where(id => !products.ContainsKey(id)).ToList();
        if (missing.Count > 0)
        {
            return Fail($"Không tìm thấy sản phẩm có Id: {string.Join(", ", missing)}.", 404);
        }

        foreach (var item in items)
        {
            var sp = products[item.SanPhamId];
            if (sp.SoLuongTon < item.SoLuong)
            {
                return Fail($"Sản phẩm '{sp.TenSP}' không đủ tồn kho (còn {sp.SoLuongTon}, cần {item.SoLuong}).", 409);
            }
        }

        // ---- Khách hàng (UC09 / UC10): kiểm tra tồn tại, điểm đổi & lấy tỷ lệ tích điểm ----
        if (req.DiemSuDung < 0)
        {
            return Fail("Số điểm sử dụng không hợp lệ.");
        }
        if (req.DiemSuDung > 0 && !req.KhachHangId.HasValue)
        {
            return Fail("Khách vãng lai không thể sử dụng điểm tích lũy.");
        }

        var tyLeTichDiem = 0.01m;
        if (req.KhachHangId.HasValue)
        {
            var khachHang = await _context.KhachHangs
                .AsNoTracking()
                .FirstOrDefaultAsync(k => k.Id == req.KhachHangId.Value, cancellationToken);
            if (khachHang == null)
            {
                return Fail($"Không tìm thấy khách hàng #{req.KhachHangId}.", 404);
            }
            if (req.DiemSuDung > khachHang.DiemTichLuy)
            {
                return Fail($"Khách hàng chỉ còn {khachHang.DiemTichLuy} điểm, không đủ {req.DiemSuDung} điểm để đổi.");
            }

            var tyLeParam = await _context.ThamSos
                .AsNoTracking()
                .FirstOrDefaultAsync(t => t.TenThamSo == "TyLeTichDiem", cancellationToken);
            if (tyLeParam != null && decimal.TryParse(tyLeParam.GiaTri, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out var parsedRate) && parsedRate >= 0)
            {
                tyLeTichDiem = parsedRate;
            }
        }

        // ---- Lập đơn (giá lấy từ DB, không tin giá client gửi lên) ----
        var donHang = new DonHang
        {
            CaLamViecId = caLamViecId,
            KhachHangId = req.KhachHangId,
            NguoiDungId = command.NguoiDungId,
            HinhThucThanhToanId = method.Id,
            NgayTao = DateTime.SpecifyKind(DateTime.Now, DateTimeKind.Unspecified),
            TrangThai = TrangThaiDonHang.DaThanhToan
        };

        foreach (var item in items)
        {
            var sp = products[item.SanPhamId];
            donHang.ChiTietDonHangs.Add(new ChiTietDonHang
            {
                SanPhamId = sp.Id,
                SoLuong = item.SoLuong,
                DonGia = sp.GiaBan,
                ThanhTien = sp.GiaBan * item.SoLuong
            });
        }

        var tongTienHang = donHang.TinhTongTienHang();
        var soTienGiam = 0m;

        // ---- Mã giảm giá (UC05): kiểm tra lại ở server ----
        MaGiamGia? voucher = null;
        if (!string.IsNullOrWhiteSpace(req.MaGiamGia))
        {
            var code = req.MaGiamGia.Trim().ToUpper();
            voucher = await _context.MaGiamGias
                .AsNoTracking()
                .FirstOrDefaultAsync(v => v.MaCode.ToUpper() == code, cancellationToken);

            if (voucher == null)
            {
                return Fail($"Mã giảm giá '{req.MaGiamGia.Trim()}' không tồn tại.");
            }
            if (!voucher.KiemTraHieuLuc(DateTime.UtcNow))
            {
                return Fail($"Mã giảm giá '{voucher.MaCode}' đã hết hạn hoặc chưa đến ngày áp dụng.");
            }

            soTienGiam = voucher.TinhTienGiam(tongTienHang);
            donHang.MaGiamGiaId = voucher.Id;
        }

        if (req.DiemSuDung > 0)
        {
            soTienGiam += req.DiemSuDung * 100m;
        }

        donHang.TongTienThanhToan = Math.Max(0, tongTienHang - soTienGiam);

        // ---- Tiền mặt: kiểm tra tiền khách đưa ----
        var isCash = PaymentMethodMatcher.IsCash(method);
        if (isCash)
        {
            if (!req.TienKhachDua.HasValue)
            {
                return Fail("Vui lòng nhập số tiền khách đưa khi thanh toán tiền mặt.");
            }
            if (req.TienKhachDua.Value < donHang.TongTienThanhToan)
            {
                return Fail(string.Format(ViVn, "Tiền khách đưa ({0:N0} đ) chưa đủ tổng thanh toán ({1:N0} đ).", req.TienKhachDua.Value, donHang.TongTienThanhToan));
            }
        }

        // ---- Transaction: lưu đơn -> lưu chi tiết -> trừ kho ----
        var tonConLai = new Dictionary<int, int>();
        await using (var transaction = await _context.Database.BeginTransactionAsync(cancellationToken))
        {
            try
            {
                // 1 + 2. Lưu DonHang rồi ChiTietDonHang (EF tự sắp thứ tự insert theo khóa ngoại)
                _context.DonHangs.Add(donHang);
                await _context.SaveChangesAsync(cancellationToken);

                // 3. Trừ tồn kho nguyên tử, chỉ thành công khi còn đủ hàng
                foreach (var item in items)
                {
                    var affected = await _context.SanPhams
                        .Where(p => p.Id == item.SanPhamId && p.SoLuongTon >= item.SoLuong)
                        .ExecuteUpdateAsync(s => s.SetProperty(p => p.SoLuongTon, p => p.SoLuongTon - item.SoLuong), cancellationToken);

                    if (affected == 0)
                    {
                        await transaction.RollbackAsync(cancellationToken);
                        return Fail($"Sản phẩm '{products[item.SanPhamId].TenSP}' vừa hết hàng, không đủ tồn kho để thanh toán.", 409);
                    }

                    tonConLai[item.SanPhamId] = products[item.SanPhamId].SoLuongTon - item.SoLuong;
                }

                // 4. Trừ điểm đã đổi (nếu có) và cộng điểm tích lũy mới cho khách hàng thành viên (UC10: 1% giá trị đơn, quy đổi 1 điểm = 100 đ)
                if (req.KhachHangId.HasValue)
                {
                    var diemCong = (int)Math.Floor(donHang.TongTienThanhToan * tyLeTichDiem / 100m);
                    var netDiem = diemCong - req.DiemSuDung;
                    if (netDiem != 0 || req.DiemSuDung > 0)
                    {
                        var updated = await _context.KhachHangs
                            .Where(k => k.Id == req.KhachHangId.Value && k.DiemTichLuy >= req.DiemSuDung)
                            .ExecuteUpdateAsync(s => s.SetProperty(k => k.DiemTichLuy, k => k.DiemTichLuy + netDiem), cancellationToken);

                        if (updated == 0 && req.DiemSuDung > 0)
                        {
                            await transaction.RollbackAsync(cancellationToken);
                            return Fail("Điểm tích lũy của khách hàng không đủ để đổi.", 409);
                        }
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

        // Gắn navigation để trả về DTO đầy đủ
        donHang.HinhThucThanhToan = method;
        donHang.MaGiamGia = voucher;
        foreach (var ct in donHang.ChiTietDonHangs)
        {
            ct.SanPham = products[ct.SanPhamId];
        }

        var dto = OrderMapper.ToDto(donHang, isCash ? req.TienKhachDua : null, tonConLai);
        return OrderOperationResult<OrderDto>.Success(dto, 201, "Thanh toán thành công, đã trừ tồn kho.");
    }

    private static OrderOperationResult<OrderDto> Fail(string message, int statusCode = 400) =>
        OrderOperationResult<OrderDto>.Failure(message, statusCode);
}
