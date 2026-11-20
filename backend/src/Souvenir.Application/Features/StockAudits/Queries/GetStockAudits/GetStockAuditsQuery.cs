using MediatR;
using Souvenir.Application.DTOs.StockAudits;

namespace Souvenir.Application.Features.StockAudits.Queries.GetStockAudits;

/// <summary>Lấy danh sách phiếu kiểm kê kho (UC13), tùy chọn lọc theo trạng thái.</summary>
public record GetStockAuditsQuery(string? TrangThai = null) : IRequest<List<StockAuditDto>>;
