using MediatR;
using Microsoft.EntityFrameworkCore;
using Souvenir.Application.DTOs.Orders;
using Souvenir.Application.Interfaces;

namespace Souvenir.Application.Features.Orders.Queries.GetPaymentMethods;

public class GetPaymentMethodsQueryHandler : IRequestHandler<GetPaymentMethodsQuery, List<PaymentMethodDto>>
{
    private readonly IApplicationDbContext _context;

    public GetPaymentMethodsQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public Task<List<PaymentMethodDto>> Handle(GetPaymentMethodsQuery request, CancellationToken cancellationToken)
    {
        return _context.HinhThucThanhToans
            .AsNoTracking()
            .OrderBy(h => h.Id)
            .Select(h => new PaymentMethodDto { Id = h.Id, TenHinhThuc = h.TenHinhThuc, TrangThai = h.TrangThai })
            .ToListAsync(cancellationToken);
    }
}
