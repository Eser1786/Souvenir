using MediatR;
using Souvenir.Application.DTOs.Returns;

namespace Souvenir.Application.Features.Returns.Queries.GetReturns;

/// <summary>Danh sách phiếu trả (lọc theo đơn gốc nếu có). Id != null → lấy một phiếu.</summary>
public record GetReturnsQuery(int? Id = null, int? DonHangId = null) : IRequest<List<ReturnDto>>;
