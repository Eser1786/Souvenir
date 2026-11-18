using MediatR;
using Souvenir.Application.DTOs.Orders;

namespace Souvenir.Application.Features.DraftOrders.Commands.DeleteDraftOrder;

public record DeleteDraftOrderCommand(int Id) : IRequest<OrderOperationResult<bool>>;
