using System.Text.RegularExpressions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Souvenir.Application.DTOs.Customers;
using Souvenir.Application.Interfaces;

namespace Souvenir.Application.Features.Customers.Commands.UpdateCustomer;

public class UpdateCustomerCommandHandler : IRequestHandler<UpdateCustomerCommand, CustomerResult<CustomerDto>>
{
    private readonly IApplicationDbContext _context;

    public UpdateCustomerCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<CustomerResult<CustomerDto>> Handle(UpdateCustomerCommand request, CancellationToken cancellationToken)
    {
        var customer = await _context.KhachHangs
            .FirstOrDefaultAsync(c => c.Id == request.Id, cancellationToken);

        if (customer == null)
        {
            return CustomerResult<CustomerDto>.Failed($"Không tìm thấy khách hàng với mã #{request.Id}.", 404);
        }

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

        if (request.DiemTichLuy < 0)
        {
            return CustomerResult<CustomerDto>.Failed("Điểm tích lũy không được là số âm.", 400);
        }

        var phoneExists = await _context.KhachHangs
            .AnyAsync(c => c.SoDienThoai == cleanPhone && c.Id != request.Id, cancellationToken);

        if (phoneExists)
        {
            return CustomerResult<CustomerDto>.Failed(
                $"Số điện thoại {cleanPhone} đã được sử dụng bởi khách hàng khác.", 409);
        }

        customer.SoDienThoai = cleanPhone;
        customer.TenKhachHang = request.TenKhachHang.Trim();
        customer.DiemTichLuy = request.DiemTichLuy;

        await _context.SaveChangesAsync(cancellationToken);

        var dto = new CustomerDto
        {
            Id = customer.Id,
            SoDienThoai = customer.SoDienThoai,
            TenKhachHang = customer.TenKhachHang,
            DiemTichLuy = customer.DiemTichLuy
        };

        return CustomerResult<CustomerDto>.Success(dto, "Cập nhật thông tin khách hàng thành công.");
    }
}
