using MediatR;
using Souvenir.Application.DTOs.Orders;

namespace Souvenir.Application.Features.Orders.Queries.GetOrderById;

public record GetOrderByIdQuery(int Id) : IRequest<OrderDto?>;
