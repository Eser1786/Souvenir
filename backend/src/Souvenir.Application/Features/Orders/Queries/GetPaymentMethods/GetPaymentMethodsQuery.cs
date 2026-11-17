using MediatR;
using Souvenir.Application.DTOs.Orders;

namespace Souvenir.Application.Features.Orders.Queries.GetPaymentMethods;

public record GetPaymentMethodsQuery : IRequest<List<PaymentMethodDto>>;
