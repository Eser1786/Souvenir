using MediatR;
using Souvenir.Application.DTOs.Customers;

namespace Souvenir.Application.Features.Customers.Queries.GetCustomers;

public record GetCustomersQuery(string? Search, string? Phone) : IRequest<List<CustomerDto>>;
