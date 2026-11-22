using MediatR;
using Souvenir.Application.DTOs.StockAudits;
using Souvenir.Application.Interfaces;

namespace Souvenir.Application.Features.StockAudits.Commands.BalanceStockAudit;

public class BalanceStockAuditCommandHandler
    : IRequestHandler<BalanceStockAuditCommand, StockAuditResult<StockAuditDto>>
{
    private readonly IApplicationDbContext _context;

    public BalanceStockAuditCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public Task<StockAuditResult<StockAuditDto>> Handle(
        BalanceStockAuditCommand command,
        CancellationToken cancellationToken)
    {
        return StockAuditHelper.SaveOrBalanceAsync(
            _context,
            command.NguoiDungId,
            command.Request,
            isBalance: true,
            cancellationToken);
    }
}
