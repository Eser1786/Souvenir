using MediatR;
using Microsoft.EntityFrameworkCore;
using Souvenir.Application.DTOs.Products;
using Souvenir.Application.Interfaces;
using Souvenir.Domain.Entities;

namespace Souvenir.Application.Features.Products.Commands.CreateProduct;

public class CreateProductCommandHandler : IRequestHandler<CreateProductCommand, ProductResult<ProductDto>>
{
    private readonly IApplicationDbContext _context;

    public CreateProductCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<ProductResult<ProductDto>> Handle(CreateProductCommand command, CancellationToken cancellationToken)
    {
        var req = command.Request;
        if (string.IsNullOrWhiteSpace(req.TenSP))
        {
            return ProductResult<ProductDto>.Failed("Tên sản phẩm không được để trống.", 400);
        }

        if (req.GiaNhap < 0)
        {
            return ProductResult<ProductDto>.Failed("Giá nhập không được âm.", 400);
        }

        if (req.GiaBan < 0)
        {
            return ProductResult<ProductDto>.Failed("Giá bán không được âm.", 400);
        }

        if (req.SoLuongTon < 0)
        {
            return ProductResult<ProductDto>.Failed("Số lượng tồn kho không được âm.", 400);
        }

        string maSP = (req.MaSP ?? string.Empty).Trim().ToUpper();
        if (string.IsNullOrWhiteSpace(maSP))
        {
            var totalCount = await _context.SanPhams.CountAsync(cancellationToken);
            maSP = $"SP{(totalCount + 1):D3}";
            while (await _context.SanPhams.AnyAsync(p => p.MaSP == maSP, cancellationToken))
            {
                totalCount++;
                maSP = $"SP{(totalCount + 1):D3}";
            }
        }
        else
        {
            var exists = await _context.SanPhams.AnyAsync(p => p.MaSP == maSP, cancellationToken);
            if (exists)
            {
                return ProductResult<ProductDto>.Failed($"Mã sản phẩm '{maSP}' đã tồn tại trong hệ thống.", 409);
            }
        }

        int danhMucId = req.DanhMucId ?? 0;
        if (danhMucId <= 0)
        {
            if (!string.IsNullOrWhiteSpace(req.TenDanhMuc))
            {
                var trimmedCat = req.TenDanhMuc.Trim();
                var dm = await _context.DanhMucSanPhams
                    .FirstOrDefaultAsync(d => d.TenDanhMuc.ToLower() == trimmedCat.ToLower(), cancellationToken);
                if (dm != null)
                {
                    danhMucId = dm.Id;
                }
                else
                {
                    var newDm = new DanhMucSanPham { TenDanhMuc = trimmedCat, TrangThai = "HoatDong" };
                    _context.DanhMucSanPhams.Add(newDm);
                    await _context.SaveChangesAsync(cancellationToken);
                    danhMucId = newDm.Id;
                }
            }
            else
            {
                var defaultDm = await _context.DanhMucSanPhams.FirstOrDefaultAsync(cancellationToken);
                danhMucId = defaultDm?.Id ?? 1;
            }
        }

        int donViTinhId = req.DonViTinhId ?? 0;
        if (donViTinhId <= 0)
        {
            if (!string.IsNullOrWhiteSpace(req.TenDonVi))
            {
                var trimmedDvt = req.TenDonVi.Trim();
                var dvt = await _context.DonViTinhs
                    .FirstOrDefaultAsync(d => d.TenDonVi.ToLower() == trimmedDvt.ToLower(), cancellationToken);
                if (dvt != null)
                {
                    donViTinhId = dvt.Id;
                }
                else
                {
                    var newDvt = new DonViTinh { TenDonVi = trimmedDvt };
                    _context.DonViTinhs.Add(newDvt);
                    await _context.SaveChangesAsync(cancellationToken);
                    donViTinhId = newDvt.Id;
                }
            }
            else
            {
                var defaultDvt = await _context.DonViTinhs.FirstOrDefaultAsync(cancellationToken);
                donViTinhId = defaultDvt?.Id ?? 1;
            }
        }

        var product = new SanPham
        {
            MaSP = maSP,
            TenSP = req.TenSP.Trim(),
            DanhMucId = danhMucId,
            DonViTinhId = donViTinhId,
            GiaNhap = req.GiaNhap,
            GiaBan = req.GiaBan,
            SoLuongTon = req.SoLuongTon,
            HinhAnh = string.IsNullOrWhiteSpace(req.ImageUrl) ? null : req.ImageUrl.Trim(),
            TrangThai = "KinhDoanh"
        };

        _context.SanPhams.Add(product);
        await _context.SaveChangesAsync(cancellationToken);

        var dmEntity = await _context.DanhMucSanPhams.FindAsync(new object[] { danhMucId }, cancellationToken);
        var dvtEntity = await _context.DonViTinhs.FindAsync(new object[] { donViTinhId }, cancellationToken);

        var dto = new ProductDto
        {
            Id = product.Id,
            MaSP = product.MaSP,
            TenSP = product.TenSP,
            DanhMucId = product.DanhMucId,
            TenDanhMuc = dmEntity?.TenDanhMuc ?? string.Empty,
            DonViTinhId = product.DonViTinhId,
            TenDonVi = dvtEntity?.TenDonVi,
            GiaNhap = product.GiaNhap,
            GiaBan = product.GiaBan,
            SoLuongTon = product.SoLuongTon,
            TrangThai = product.TrangThai,
            HinhAnh = product.HinhAnh
        };

        return ProductResult<ProductDto>.Created(dto, "Thêm mới sản phẩm thành công.");
    }
}
