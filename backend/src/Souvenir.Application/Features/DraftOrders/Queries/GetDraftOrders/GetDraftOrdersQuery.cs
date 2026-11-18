using MediatR;
using Souvenir.Application.DTOs.Orders;

namespace Souvenir.Application.Features.DraftOrders.Queries.GetDraftOrders;

public record GetDraftOrdersQuery() : IRequest<List<DraftOrderDto>>;
