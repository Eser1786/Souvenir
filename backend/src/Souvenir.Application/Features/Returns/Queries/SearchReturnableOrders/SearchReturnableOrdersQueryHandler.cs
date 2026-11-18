using MediatR;
using Microsoft.EntityFrameworkCore;
using Souvenir.Application.DTOs.Returns;
using Souvenir.Application.Interfaces;
using Souvenir.Domain.Entities;

namespace Souvenir.Application.Features.Returns.Queries.SearchReturnableOrders;

public class SearchReturnableOrdersQueryHandler : IRequestHandler<SearchReturnableOrdersQuery, List<ReturnableOrderDto>>
{
    private const int MaxResults = 50;
    private readonly IApplicationDbContext _context;

    public SearchReturnableOrdersQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<ReturnableOrderDto>> Handle(SearchReturnableOrdersQuery request, CancellationToken cancellationToken)
    {
        var query = _context.DonHangs.AsNoTracking()
            .Where(d => d.TrangThai == TrangThaiDonHang.DaThanhToan || d.TrangThai == TrangThaiDonHang.DaTraHang);

        var keyword = request.Keyword?.Trim();
        if (!string.IsNullOrEmpty(keyword))
        {
            var digits = new string(keyword.Where(char.IsDigit).ToArray());
            var isMaDon = keyword.StartsWith("DH", StringComparison.OrdinalIgnoreCase) || keyword.StartsWith('#');

            if (!isMaDon && digits.Length >= 8)
            {
                // Số điện thoại khách hàng (khớp một phần)
                var customerIds = _context.KhachHangs.Where(k => k.SoDienThoai.Contains(digits)).Select(k => k.Id);
                query = query.Where(d => d.KhachHangId.HasValue && customerIds.Contains(d.KhachHangId.Value));
            }
            else
            {
                var id = ReturnPolicy.ParseMaDonHang(keyword);
                if (id == null) return new List<ReturnableOrderDto>();
                query = query.Where(d => d.Id == id.Value);
            }
        }

        if (request.TuNgay.HasValue)
        {
            var from = request.TuNgay.Value.Date;
            query = query.Where(d => d.NgayTao >= from);
        }
        if (request.DenNgay.HasValue)
        {
            var to = request.DenNgay.Value.Date.AddDays(1);
            query = query.Where(d => d.NgayTao < to);
        }

        var orders = await query
            .Include(d => d.HinhThucThanhToan)
            .Include(d => d.MaGiamGia)
            .Include(d => d.ChiTietDonHangs).ThenInclude(ct => ct.SanPham)
            .OrderByDescending(d => d.NgayTao)
            .Take(MaxResults)
            .ToListAsync(cancellationToken);

        if (orders.Count == 0) return new List<ReturnableOrderDto>();

        var ids = orders.Select(o => o.Id).ToList();
        var customerIdList = orders.Where(o => o.KhachHangId.HasValue).Select(o => o.KhachHangId!.Value).Distinct().ToList();
        var customers = await _context.KhachHangs.AsNoTracking()
            .Where(k => customerIdList.Contains(k.Id))
            .ToDictionaryAsync(k => k.Id, cancellationToken);
        var daTra = await ReturnPolicy.GetSoLuongDaTraAsync(_context, ids, cancellationToken);
        var daHoan = await ReturnPolicy.GetTongDaHoanAsync(_context, ids, cancellationToken);
        var soNgay = await ReturnPolicy.GetSoNgayDoiTraAsync(_context, cancellationToken);
        var now = ReturnPolicy.Now();

        return orders.Select(o => ReturnPolicy.ToReturnableDto(
                o,
                o.KhachHangId.HasValue && customers.TryGetValue(o.KhachHangId.Value, out var kh) ? kh : null,
                soNgay,
                now,
                daTra,
                daHoan.TryGetValue(o.Id, out var tong) ? tong : 0))
            .ToList();
    }
}
