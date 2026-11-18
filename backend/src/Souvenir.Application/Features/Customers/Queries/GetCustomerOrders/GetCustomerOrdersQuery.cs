using MediatR;
using Souvenir.Application.DTOs.Customers;

namespace Souvenir.Application.Features.Customers.Queries.GetCustomerOrders;

public record GetCustomerOrdersQuery(int CustomerId) : IRequest<CustomerResult<List<CustomerOrderDto>>>;
