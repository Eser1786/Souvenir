using MediatR;
using Microsoft.EntityFrameworkCore;
using Souvenir.Application.DTOs.Returns;
using Souvenir.Application.Interfaces;

namespace Souvenir.Application.Features.Returns.Queries.GetReturnableOrder;

public class GetReturnableOrderQueryHandler : IRequestHandler<GetReturnableOrderQuery, ReturnableOrderDto?>
{
    private readonly IApplicationDbContext _context;

    public GetReturnableOrderQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<ReturnableOrderDto?> Handle(GetReturnableOrderQuery request, CancellationToken cancellationToken)
    {
        var donHang = await _context.DonHangs.AsNoTracking()
            .Include(d => d.HinhThucThanhToan)
            .Include(d => d.MaGiamGia)
            .Include(d => d.ChiTietDonHangs).ThenInclude(ct => ct.SanPham)
            .FirstOrDefaultAsync(d => d.Id == request.DonHangId, cancellationToken);
        if (donHang == null) return null;

        var khachHang = donHang.KhachHangId.HasValue
            ? await _context.KhachHangs.AsNoTracking().FirstOrDefaultAsync(k => k.Id == donHang.KhachHangId.Value, cancellationToken)
            : null;
        var ids = new List<int> { donHang.Id };
        var daTra = await ReturnPolicy.GetSoLuongDaTraAsync(_context, ids, cancellationToken);
        var daHoan = await ReturnPolicy.GetTongDaHoanAsync(_context, ids, cancellationToken);
        var soNgay = await ReturnPolicy.GetSoNgayDoiTraAsync(_context, cancellationToken);

        return ReturnPolicy.ToReturnableDto(donHang, khachHang, soNgay, ReturnPolicy.Now(), daTra,
            daHoan.TryGetValue(donHang.Id, out var tong) ? tong : 0);
    }
}
