using MediatR;
using Microsoft.EntityFrameworkCore;
using Souvenir.Application.DTOs.Orders;
using Souvenir.Application.Interfaces;

namespace Souvenir.Application.Features.Orders.Queries.GetOrderById;

public class GetOrderByIdQueryHandler : IRequestHandler<GetOrderByIdQuery, OrderDto?>
{
    private readonly IApplicationDbContext _context;

    public GetOrderByIdQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<OrderDto?> Handle(GetOrderByIdQuery request, CancellationToken cancellationToken)
    {
        var donHang = await _context.DonHangs
            .AsNoTracking()
            .Include(d => d.HinhThucThanhToan)
            .Include(d => d.MaGiamGia)
            .Include(d => d.ChiTietDonHangs).ThenInclude(ct => ct.SanPham)
            .FirstOrDefaultAsync(d => d.Id == request.Id, cancellationToken);

        return donHang == null ? null : OrderMapper.ToDto(donHang);
    }
}
