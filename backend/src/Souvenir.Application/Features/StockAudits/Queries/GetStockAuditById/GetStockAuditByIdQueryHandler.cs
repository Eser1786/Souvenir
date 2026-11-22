using MediatR;
using Microsoft.EntityFrameworkCore;
using Souvenir.Application.DTOs.StockAudits;
using Souvenir.Application.Interfaces;

namespace Souvenir.Application.Features.StockAudits.Queries.GetStockAuditById;

public class GetStockAuditByIdQueryHandler : IRequestHandler<GetStockAuditByIdQuery, StockAuditDto?>
{
    private readonly IApplicationDbContext _context;

    public GetStockAuditByIdQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<StockAuditDto?> Handle(GetStockAuditByIdQuery request, CancellationToken cancellationToken)
    {
        var phieu = await StockAuditHelper.QueryWithDetails(_context)
            .FirstOrDefaultAsync(p => p.Id == request.Id, cancellationToken);

        return phieu == null ? null : StockAuditHelper.ToDto(phieu);
    }
}
