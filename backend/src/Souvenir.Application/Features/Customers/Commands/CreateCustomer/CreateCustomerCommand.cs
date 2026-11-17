using MediatR;
using Souvenir.Application.DTOs.Customers;

namespace Souvenir.Application.Features.Customers.Commands.CreateCustomer;

public record CreateCustomerCommand(string SoDienThoai, string TenKhachHang) : IRequest<CustomerResult<CustomerDto>>;
