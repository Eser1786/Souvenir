using MediatR;
using Souvenir.Application.DTOs.Orders;

namespace Souvenir.Application.Features.DraftOrders.Commands.CreateDraftOrder;

public record CreateDraftOrderCommand(int NguoiDungId, CreateDraftOrderRequest Request)
    : IRequest<OrderOperationResult<DraftOrderDto>>;
