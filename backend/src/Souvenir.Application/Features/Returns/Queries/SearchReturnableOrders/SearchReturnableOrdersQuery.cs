using MediatR;
using Souvenir.Application.DTOs.Returns;

namespace Souvenir.Application.Features.Returns.Queries.SearchReturnableOrders;

/// <summary>Tra cứu hóa đơn gốc theo mã đơn / SĐT khách / khoảng ngày mua (UC08 - luồng 2a).</summary>
public record SearchReturnableOrdersQuery(string? Keyword, DateTime? TuNgay, DateTime? DenNgay) : IRequest<List<ReturnableOrderDto>>;
