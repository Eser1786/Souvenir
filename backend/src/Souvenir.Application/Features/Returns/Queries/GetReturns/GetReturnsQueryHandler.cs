using MediatR;
using Microsoft.EntityFrameworkCore;
using Souvenir.Application.DTOs.Returns;
using Souvenir.Application.Features.Orders;
using Souvenir.Application.Interfaces;

namespace Souvenir.Application.Features.Returns.Queries.GetReturns;

public class GetReturnsQueryHandler : IRequestHandler<GetReturnsQuery, List<ReturnDto>>
{
    private readonly IApplicationDbContext _context;

    public GetReturnsQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<ReturnDto>> Handle(GetReturnsQuery request, CancellationToken cancellationToken)
    {
        var query = _context.PhieuTraHangs.AsNoTracking();
        if (request.Id.HasValue) query = query.Where(p => p.Id == request.Id.Value);
        if (request.DonHangId.HasValue) query = query.Where(p => p.DonHangGocId == request.DonHangId.Value);

        var phieus = await query
            .Include(p => p.NguoiDung)
            .Include(p => p.DonHangGoc).ThenInclude(d => d!.HinhThucThanhToan)
            .Include(p => p.ChiTietTraHangs).ThenInclude(ct => ct.SanPham)
            .OrderByDescending(p => p.NgayTra)
            .Take(100)
            .ToListAsync(cancellationToken);

        return phieus.Select(p => new ReturnDto
        {
            Id = p.Id,
            MaPhieu = p.MaPhieu,
            DonHangGocId = p.DonHangGocId,
            MaDonHangGoc = p.DonHangGoc != null ? OrderMapper.FormatMaDonHang(p.DonHangGoc) : string.Empty,
            NgayTra = p.NgayTra,
            NguoiDungId = p.NguoiDungId,
            TenNguoiLap = p.NguoiDung?.HoTen,
            LyDoTra = p.LyDoTra,
            TongTienHoan = p.TongTienHoan,
            HinhThucHoan = p.DonHangGoc?.HinhThucThanhToan?.TenHinhThuc ?? string.Empty,
            TrangThaiDonGoc = p.DonHangGoc?.TrangThai ?? string.Empty,
            Items = p.ChiTietTraHangs.Select(ct => new ReturnItemDto
            {
                SanPhamId = ct.SanPhamId,
                MaSP = ct.SanPham?.MaSP ?? string.Empty,
                TenSP = ct.SanPham?.TenSP ?? string.Empty,
                SoLuongTra = ct.SoLuongTra,
                DonGiaHoan = ct.DonGiaHoan,
                ThanhTien = ct.DonGiaHoan * ct.SoLuongTra,
                TinhTrangHang = ct.TinhTrangHang
            }).ToList()
        }).ToList();
    }
}
