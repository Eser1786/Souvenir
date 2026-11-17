using MediatR;
using Microsoft.EntityFrameworkCore;
using Souvenir.Application.DTOs.Customers;
using Souvenir.Application.Interfaces;

namespace Souvenir.Application.Features.Customers.Queries.GetCustomerById;

public class GetCustomerByIdQueryHandler : IRequestHandler<GetCustomerByIdQuery, CustomerDto?>
{
    private readonly IApplicationDbContext _context;

    public GetCustomerByIdQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<CustomerDto?> Handle(GetCustomerByIdQuery request, CancellationToken cancellationToken)
    {
        var customer = await _context.KhachHangs
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.Id == request.Id, cancellationToken);

        if (customer == null)
            return null;

        return new CustomerDto
        {
            Id = customer.Id,
            SoDienThoai = customer.SoDienThoai,
            TenKhachHang = customer.TenKhachHang,
            DiemTichLuy = customer.DiemTichLuy
        };
    }
}
