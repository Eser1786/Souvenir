using MediatR;
using Souvenir.Application.DTOs.Customers;

namespace Souvenir.Application.Features.Customers.Commands.DeleteCustomer;

public record DeleteCustomerCommand(int Id) : IRequest<CustomerResult<bool>>;
