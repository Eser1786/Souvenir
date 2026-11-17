using MediatR;
using Souvenir.Application.DTOs.Orders;

namespace Souvenir.Application.Features.Orders.Commands.CreateOrder;

/// <summary>Tạo đơn hàng đã thanh toán tại quầy POS (UC07).</summary>
public record CreateOrderCommand(int NguoiDungId, CreateOrderRequest Request) : IRequest<OrderOperationResult<OrderDto>>;
