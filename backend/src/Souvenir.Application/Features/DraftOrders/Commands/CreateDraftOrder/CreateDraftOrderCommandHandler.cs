using MediatR;
using Microsoft.EntityFrameworkCore;
using Souvenir.Application.DTOs.Orders;
using Souvenir.Application.Interfaces;
using Souvenir.Domain.Entities;
using Souvenir.Domain.Enums;

namespace Souvenir.Application.Features.DraftOrders.Commands.CreateDraftOrder;

public class CreateDraftOrderCommandHandler : IRequestHandler<CreateDraftOrderCommand, OrderOperationResult<DraftOrderDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly ICaLamViecLookup _caLamViecLookup;

    public CreateDraftOrderCommandHandler(IApplicationDbContext context, ICaLamViecLookup caLamViecLookup)
    {
        _context = context;
        _caLamViecLookup = caLamViecLookup;
    }

    public async Task<OrderOperationResult<DraftOrderDto>> Handle(CreateDraftOrderCommand command, CancellationToken cancellationToken)
    {
        var req = command.Request;
        if (req == null || req.Items == null || req.Items.Count == 0)
        {
            return OrderOperationResult<DraftOrderDto>.Failure("Giỏ hàng trống, không thể lưu nháp đơn.", 400);
        }

        if (req.Items.Any(i => i.SoLuong <= 0))
        {
            return OrderOperationResult<DraftOrderDto>.Failure("Số lượng mỗi sản phẩm phải lớn hơn 0.", 400);
        }

        // Gộp sản phẩm trùng
        var items = req.Items
            .GroupBy(i => i.SanPhamId)
            .Select(g => new { SanPhamId = g.Key, SoLuong = g.Sum(x => x.SoLuong) })
            .ToList();

        var productIds = items.Select(i => i.SanPhamId).ToList();
        var products = await _context.SanPhams
            .AsNoTracking()
            .Where(p => productIds.Contains(p.Id))
            .ToDictionaryAsync(p => p.Id, cancellationToken);

        var missing = productIds.Where(id => !products.ContainsKey(id)).ToList();
        if (missing.Count > 0)
        {
            return OrderOperationResult<DraftOrderDto>.Failure($"Không tìm thấy sản phẩm có Id: {string.Join(", ", missing)}.", 404);
        }

        // Tính tổng tiền hàng
        decimal tongTienHang = items.Sum(i => products[i.SanPhamId].GiaBan * i.SoLuong);
        decimal soTienGiam = 0m;
        int? maGiamGiaId = null;
        string? maGiamGiaCode = null;

        // Xử lý mã giảm giá nếu có
        if (!string.IsNullOrWhiteSpace(req.MaGiamGia))
        {
            var voucher = await _context.MaGiamGias
                .AsNoTracking()
                .FirstOrDefaultAsync(v => v.MaCode.ToLower() == req.MaGiamGia.Trim().ToLower(), cancellationToken);

            if (voucher != null)
            {
                maGiamGiaId = voucher.Id;
                maGiamGiaCode = voucher.MaCode;
                soTienGiam += voucher.TinhTienGiam(tongTienHang);
            }
        }

        // Xử lý điểm tích lũy đổi tiền (50 điểm = 5.000đ -> 1 điểm = 100đ)
        if (req.DiemSuDung > 0)
        {
            soTienGiam += req.DiemSuDung * 100m;
        }

        if (soTienGiam > tongTienHang) soTienGiam = tongTienHang;
        decimal tongTienThanhToan = Math.Max(0m, tongTienHang - soTienGiam);

        // Xác định ca làm việc
        int caLamViecId;
        if (req.CaLamViecId.HasValue && req.CaLamViecId.Value > 0)
        {
            caLamViecId = req.CaLamViecId.Value;
        }
        else
        {
            var openShift = await _caLamViecLookup.GetOpenShiftIdAsync(command.NguoiDungId, cancellationToken);
            if (openShift.HasValue)
            {
                caLamViecId = openShift.Value;
            }
            else
            {
                var anyShift = await _context.CaLamViecs.Select(c => c.Id).OrderByDescending(id => id).FirstOrDefaultAsync(cancellationToken);
                if (anyShift > 0)
                {
                    caLamViecId = anyShift;
                }
                else
                {
                    var defaultShift = new CaLamViec
                    {
                        NguoiDungId = command.NguoiDungId,
                        ThoiGianMoCa = DateTime.UtcNow,
                        TienMatBanGiao = 500000m,
                        TrangThai = nameof(TrangThaiCaLamViec.DANG_MO)
                    };
                    _context.CaLamViecs.Add(defaultShift);
                    await _context.SaveChangesAsync(cancellationToken);
                    caLamViecId = defaultShift.Id;
                }
            }
        }

        // Hình thức thanh toán mặc định (Tiền mặt = 1)
        var defaultPayment = await _context.HinhThucThanhToans.Select(h => h.Id).FirstOrDefaultAsync(cancellationToken);
        int paymentMethodId = defaultPayment > 0 ? defaultPayment : 1;

        // Lưu đơn nháp vào DonHang với TrangThai = LuuNhap (KHÔNG trừ tồn kho)
        var donHang = new DonHang
        {
            CaLamViecId = caLamViecId,
            NguoiDungId = command.NguoiDungId,
            KhachHangId = req.KhachHangId,
            MaGiamGiaId = maGiamGiaId,
            HinhThucThanhToanId = paymentMethodId,
            NgayTao = DateTime.SpecifyKind(DateTime.Now, DateTimeKind.Unspecified),
            TongTienThanhToan = tongTienThanhToan,
            TrangThai = TrangThaiDonHang.LuuNhap
        };

        _context.DonHangs.Add(donHang);
        await _context.SaveChangesAsync(cancellationToken);

        var detailDtos = new List<DraftOrderItemDto>();
        foreach (var item in items)
        {
            var sp = products[item.SanPhamId];
            var ct = new ChiTietDonHang
            {
                DonHangId = donHang.Id,
                SanPhamId = sp.Id,
                SoLuong = item.SoLuong,
                DonGia = sp.GiaBan,
                ThanhTien = sp.GiaBan * item.SoLuong
            };
            _context.ChiTietDonHangs.Add(ct);

            detailDtos.Add(new DraftOrderItemDto
            {
                SanPhamId = sp.Id,
                MaSP = sp.MaSP,
                TenSP = sp.TenSP,
                ImageUrl = sp.HinhAnh,
                SoLuong = item.SoLuong,
                DonGia = sp.GiaBan,
                ThanhTien = ct.ThanhTien,
                SoLuongTon = sp.SoLuongTon
            });
        }

        await _context.SaveChangesAsync(cancellationToken);

        // Lấy thông tin khách hàng nếu có
        string? tenKhachHang = null;
        string? sdtKhach = null;
        if (req.KhachHangId.HasValue)
        {
            var kh = await _context.KhachHangs.AsNoTracking().FirstOrDefaultAsync(k => k.Id == req.KhachHangId.Value, cancellationToken);
            if (kh != null)
            {
                tenKhachHang = kh.TenKhachHang;
                sdtKhach = kh.SoDienThoai;
            }
        }

        var user = await _context.NguoiDungs.AsNoTracking().FirstOrDefaultAsync(u => u.Id == command.NguoiDungId, cancellationToken);

        var resultDto = new DraftOrderDto
        {
            Id = donHang.Id,
            MaDonHang = $"NHAP-{donHang.Id:D5}",
            NgayTao = donHang.NgayTao,
            TrangThai = TrangThaiDonHang.LuuNhap,
            NguoiDungId = command.NguoiDungId,
            TenNhanVien = user?.HoTen ?? user?.TenDangNhap,
            KhachHangId = req.KhachHangId,
            TenKhachHang = tenKhachHang,
            SoDienThoaiKhach = sdtKhach,
            MaGiamGia = maGiamGiaCode,
            DiemSuDung = req.DiemSuDung,
            TongTienHang = tongTienHang,
            SoTienGiam = soTienGiam,
            TongTienThanhToan = tongTienThanhToan,
            TongSoLuongMon = items.Sum(i => i.SoLuong),
            Items = detailDtos
        };

        return OrderOperationResult<DraftOrderDto>.Success(resultDto, 201, "Lưu đơn nháp thành công.");
    }
}
