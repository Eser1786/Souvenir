using MediatR;
using Souvenir.Application.DTOs.StockAudits;
using Souvenir.Application.Interfaces;

namespace Souvenir.Application.Features.StockAudits.Commands.SaveDraftStockAudit;

public class SaveDraftStockAuditCommandHandler
    : IRequestHandler<SaveDraftStockAuditCommand, StockAuditResult<StockAuditDto>>
{
    private readonly IApplicationDbContext _context;

    public SaveDraftStockAuditCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public Task<StockAuditResult<StockAuditDto>> Handle(
        SaveDraftStockAuditCommand command,
        CancellationToken cancellationToken)
    {
        return StockAuditHelper.SaveOrBalanceAsync(
            _context,
            command.NguoiDungId,
            command.Request,
            isBalance: false,
            cancellationToken);
    }
}
