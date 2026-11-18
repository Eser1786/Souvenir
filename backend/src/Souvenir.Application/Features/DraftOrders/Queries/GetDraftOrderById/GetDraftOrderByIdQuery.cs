using MediatR;
using Souvenir.Application.DTOs.Orders;

namespace Souvenir.Application.Features.DraftOrders.Queries.GetDraftOrderById;

public record GetDraftOrderByIdQuery(int Id) : IRequest<DraftOrderDto?>;
