using MediatR;
using Souvenir.Application.DTOs.Customers;

namespace Souvenir.Application.Features.Customers.Queries.GetCustomerById;

public record GetCustomerByIdQuery(int Id) : IRequest<CustomerDto?>;
