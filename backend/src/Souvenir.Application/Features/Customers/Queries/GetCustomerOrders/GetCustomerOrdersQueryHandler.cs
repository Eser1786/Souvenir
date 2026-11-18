using MediatR;
using Microsoft.EntityFrameworkCore;
using Souvenir.Application.DTOs.Customers;
using Souvenir.Application.Features.Orders;
using Souvenir.Application.Interfaces;

namespace Souvenir.Application.Features.Customers.Queries.GetCustomerOrders;

public class GetCustomerOrdersQueryHandler : IRequestHandler<GetCustomerOrdersQuery, CustomerResult<List<CustomerOrderDto>>>
{
    private readonly IApplicationDbContext _context;

    public GetCustomerOrdersQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<CustomerResult<List<CustomerOrderDto>>> Handle(
        GetCustomerOrdersQuery request,
        CancellationToken cancellationToken)
    {
        var customerExists = await _context.KhachHangs
            .AsNoTracking()
            .AnyAsync(c => c.Id == request.CustomerId, cancellationToken);

        if (!customerExists)
        {
            return CustomerResult<List<CustomerOrderDto>>.Failed(
                $"Không tìm thấy khách hàng với mã #{request.CustomerId}.", 404);
        }

        var donHangs = await _context.DonHangs
            .AsNoTracking()
            .Include(d => d.HinhThucThanhToan)
            .Include(d => d.MaGiamGia)
            .Include(d => d.NguoiDung)
            .Include(d => d.ChiTietDonHangs)
                .ThenInclude(ct => ct.SanPham)
            .Where(d => d.KhachHangId == request.CustomerId)
            .OrderByDescending(d => d.NgayTao)
            .ThenByDescending(d => d.Id)
            .ToListAsync(cancellationToken);

        var result = donHangs.Select(d =>
        {
            var tongTienHang = d.TinhTongTienHang();
            return new CustomerOrderDto
            {
                Id = d.Id,
                MaDonHang = OrderMapper.FormatMaDonHang(d),
                NgayTao = d.NgayTao,
                TongTienHang = tongTienHang,
                SoTienGiam = Math.Max(0, tongTienHang - d.TongTienThanhToan),
                TongTienThanhToan = d.TongTienThanhToan,
                TrangThai = d.TrangThai,
                TenHinhThucThanhToan = d.HinhThucThanhToan?.TenHinhThuc ?? string.Empty,
                MaGiamGia = d.MaGiamGia?.MaCode,
                TenNhanVien = d.NguoiDung?.HoTen ?? string.Empty,
                SoLuongSanPham = d.ChiTietDonHangs.Sum(ct => ct.SoLuong),
                Items = d.ChiTietDonHangs.Select(ct => new CustomerOrderItemDto
                {
                    SanPhamId = ct.SanPhamId,
                    MaSP = ct.SanPham?.MaSP ?? string.Empty,
                    TenSP = ct.SanPham?.TenSP ?? string.Empty,
                    SoLuong = ct.SoLuong,
                    DonGia = ct.DonGia,
                    ThanhTien = ct.ThanhTien
                }).ToList()
            };
        }).ToList();

        return CustomerResult<List<CustomerOrderDto>>.Success(result);
    }
}
