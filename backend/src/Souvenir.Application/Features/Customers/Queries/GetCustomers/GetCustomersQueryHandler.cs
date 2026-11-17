using MediatR;
using Microsoft.EntityFrameworkCore;
using Souvenir.Application.DTOs.Customers;
using Souvenir.Application.Interfaces;

namespace Souvenir.Application.Features.Customers.Queries.GetCustomers;

public class GetCustomersQueryHandler : IRequestHandler<GetCustomersQuery, List<CustomerDto>>
{
    private readonly IApplicationDbContext _context;

    public GetCustomersQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<CustomerDto>> Handle(GetCustomersQuery request, CancellationToken cancellationToken)
    {
        var query = _context.KhachHangs.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(request.Phone))
        {
            var cleanPhone = request.Phone.Trim().Replace(" ", "");
            query = query.Where(c => c.SoDienThoai.Contains(cleanPhone));
        }

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var search = request.Search.Trim().ToLower().Replace(" ", "");
            query = query.Where(c => c.TenKhachHang.ToLower().Contains(request.Search.Trim().ToLower()) ||
                                     c.SoDienThoai.Contains(search));
        }

        var customers = await query
            .OrderByDescending(c => c.Id)
            .Select(c => new CustomerDto
            {
                Id = c.Id,
                SoDienThoai = c.SoDienThoai,
                TenKhachHang = c.TenKhachHang,
                DiemTichLuy = c.DiemTichLuy
            })
            .ToListAsync(cancellationToken);

        return customers;
    }
}
