using MediatR;
using Souvenir.Application.DTOs.Customers;

namespace Souvenir.Application.Features.Customers.Commands.UpdateCustomer;

public record UpdateCustomerCommand(int Id, string SoDienThoai, string TenKhachHang, int DiemTichLuy) : IRequest<CustomerResult<CustomerDto>>;
