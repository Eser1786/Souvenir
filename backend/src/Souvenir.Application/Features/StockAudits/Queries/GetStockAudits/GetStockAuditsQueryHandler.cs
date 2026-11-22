using MediatR;
using Microsoft.EntityFrameworkCore;
using Souvenir.Application.DTOs.StockAudits;
using Souvenir.Application.Interfaces;

namespace Souvenir.Application.Features.StockAudits.Queries.GetStockAudits;

public class GetStockAuditsQueryHandler : IRequestHandler<GetStockAuditsQuery, List<StockAuditDto>>
{
    private readonly IApplicationDbContext _context;

    public GetStockAuditsQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<StockAuditDto>> Handle(GetStockAuditsQuery request, CancellationToken cancellationToken)
    {
        var query = StockAuditHelper.QueryWithDetails(_context);

        if (!string.IsNullOrWhiteSpace(request.TrangThai))
        {
            var status = request.TrangThai.Trim();
            query = query.Where(p => p.TrangThai == status);
        }

        var list = await query
            .OrderByDescending(p => p.NgayKiemKe)
            .ThenByDescending(p => p.Id)
            .ToListAsync(cancellationToken);

        return list.Select(StockAuditHelper.ToDto).ToList();
    }
}
