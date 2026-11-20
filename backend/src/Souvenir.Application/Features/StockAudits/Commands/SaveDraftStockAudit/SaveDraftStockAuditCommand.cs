using MediatR;
using Souvenir.Application.DTOs.StockAudits;

namespace Souvenir.Application.Features.StockAudits.Commands.SaveDraftStockAudit;

/// <summary>Lưu tạm phiếu kiểm kê kho ở trạng thái DangKiemKe (chưa thay đổi tồn kho).</summary>
public record SaveDraftStockAuditCommand(int NguoiDungId, SaveStockAuditRequest Request)
    : IRequest<StockAuditResult<StockAuditDto>>;
