using MediatR;
using Souvenir.Application.DTOs.StockAudits;

namespace Souvenir.Application.Features.StockAudits.Queries.GetStockAuditById;

/// <summary>Lấy chi tiết một phiếu kiểm kê kho theo Id (UC13).</summary>
public record GetStockAuditByIdQuery(int Id) : IRequest<StockAuditDto?>;
