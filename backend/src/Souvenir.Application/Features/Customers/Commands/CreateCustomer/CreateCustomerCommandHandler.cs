using System.Text.RegularExpressions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Souvenir.Application.DTOs.Customers;
using Souvenir.Application.Interfaces;
using Souvenir.Domain.Entities;

namespace Souvenir.Application.Features.Customers.Commands.CreateCustomer;

public class CreateCustomerCommandHandler : IRequestHandler<CreateCustomerCommand, CustomerResult<CustomerDto>>
{
    private readonly IApplicationDbContext _context;

    public CreateCustomerCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<CustomerResult<CustomerDto>> Handle(CreateCustomerCommand request, CancellationToken cancellationToken)
    {
        var cleanPhone = (request.SoDienThoai ?? string.Empty).Trim().Replace(" ", "");

        if (!Regex.IsMatch(cleanPhone, @"^0\d{9}$"))
        {
            return CustomerResult<CustomerDto>.Failed(
                "Số điện thoại không hợp lệ. Vui lòng nhập đúng 10 chữ số bắt đầu bằng số 0.", 400);
        }

        if (string.IsNullOrWhiteSpace(request.TenKhachHang))
        {
            return CustomerResult<CustomerDto>.Failed("Tên khách hàng không được để trống.", 400);
        }

        var exists = await _context.KhachHangs
            .AnyAsync(c => c.SoDienThoai == cleanPhone, cancellationToken);

        if (exists)
        {
            return CustomerResult<CustomerDto>.Failed(
                $"Số điện thoại {cleanPhone} đã được đăng ký cho khách hàng khác.", 409);
        }

        var customer = new KhachHang
        {
            SoDienThoai = cleanPhone,
            TenKhachHang = request.TenKhachHang.Trim(),
            DiemTichLuy = 0
        };

        _context.KhachHangs.Add(customer);
        await _context.SaveChangesAsync(cancellationToken);

        var dto = new CustomerDto
        {
            Id = customer.Id,
            SoDienThoai = customer.SoDienThoai,
            TenKhachHang = customer.TenKhachHang,
            DiemTichLuy = customer.DiemTichLuy
        };

        return CustomerResult<CustomerDto>.Created(dto, "Tạo mới hồ sơ khách hàng thành công.");
    }
}
