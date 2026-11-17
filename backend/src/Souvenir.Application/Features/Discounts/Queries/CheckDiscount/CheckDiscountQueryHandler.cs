using MediatR;
using Microsoft.EntityFrameworkCore;
using Souvenir.Application.DTOs.Discounts;
using Souvenir.Application.Interfaces;

namespace Souvenir.Application.Features.Discounts.Queries.CheckDiscount;

public class CheckDiscountQueryHandler : IRequestHandler<CheckDiscountQuery, CheckDiscountResponse>
{
    private readonly IApplicationDbContext _context;

    public CheckDiscountQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<CheckDiscountResponse> Handle(CheckDiscountQuery request, CancellationToken cancellationToken)
    {
        var rawCode = request.MaCode?.Trim();
        if (string.IsNullOrWhiteSpace(rawCode))
        {
            return new CheckDiscountResponse
            {
                HopLe = false,
                ThongBao = "Vui lòng nhập mã giảm giá.",
                SoTienGiam = 0,
                ThanhTienSauGiam = request.TongTienHang
            };
        }

        // Tìm kiếm mã không phân biệt hoa thường
        var voucher = await _context.MaGiamGias
            .AsNoTracking()
            .FirstOrDefaultAsync(v => v.MaCode.ToUpper() == rawCode.ToUpper(), cancellationToken);

        if (voucher == null)
        {
            return new CheckDiscountResponse
            {
                HopLe = false,
                ThongBao = $"Mã giảm giá '{rawCode}' không tồn tại trong hệ thống.",
                MaCode = rawCode,
                SoTienGiam = 0,
                ThanhTienSauGiam = request.TongTienHang
            };
        }

        // Kiểm tra thời hạn hiệu lực
        var now = DateTime.UtcNow;
        if (!voucher.KiemTraHieuLuc(now))
        {
            return new CheckDiscountResponse
            {
                HopLe = false,
                ThongBao = $"Mã giảm giá '{voucher.MaCode}' đã hết hạn hoặc chưa đến ngày áp dụng.",
                MaCode = voucher.MaCode,
                TenVoucher = voucher.TenVoucher,
                LoaiGiamGia = voucher.LoaiGiamGia,
                GiaTriGiam = voucher.GiaTriGiam,
                NgayKetThuc = voucher.NgayKetThuc,
                SoTienGiam = 0,
                ThanhTienSauGiam = request.TongTienHang
            };
        }

        // Tính tiền giảm giá
        var soTienGiam = voucher.TinhTienGiam(request.TongTienHang);
        var thanhTienSauGiam = Math.Max(0, request.TongTienHang - soTienGiam);

        return new CheckDiscountResponse
        {
            HopLe = true,
            ThongBao = $"Áp dụng mã '{voucher.MaCode}' thành công! Giảm {(voucher.LoaiGiamGia.Equals("PhanTram", StringComparison.OrdinalIgnoreCase) ? $"{voucher.GiaTriGiam}%" : $"{voucher.GiaTriGiam:N0} đ")}.",
            MaCode = voucher.MaCode,
            TenVoucher = voucher.TenVoucher,
            LoaiGiamGia = voucher.LoaiGiamGia,
            GiaTriGiam = voucher.GiaTriGiam,
            SoTienGiam = soTienGiam,
            ThanhTienSauGiam = thanhTienSauGiam,
            NgayKetThuc = voucher.NgayKetThuc
        };
    }
}
