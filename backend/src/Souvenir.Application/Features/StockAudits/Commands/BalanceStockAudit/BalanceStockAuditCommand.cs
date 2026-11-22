using MediatR;
using Souvenir.Application.DTOs.StockAudits;

namespace Souvenir.Application.Features.StockAudits.Commands.BalanceStockAudit;

/// <summary>Xác nhận cân bằng tồn kho (UC13): bắt buộc lý do khi có chênh lệch, cập nhật tồn kho thực tế và khóa phiếu.</summary>
public record BalanceStockAuditCommand(int NguoiDungId, SaveStockAuditRequest Request)
    : IRequest<StockAuditResult<StockAuditDto>>;
