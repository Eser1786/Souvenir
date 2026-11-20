using MediatR;
using Microsoft.EntityFrameworkCore;
using Souvenir.Application.DTOs.Products;
using Souvenir.Application.Interfaces;
using Souvenir.Domain.Entities;

namespace Souvenir.Application.Features.Products.Commands.UpdateProduct;

public class UpdateProductCommandHandler : IRequestHandler<UpdateProductCommand, ProductResult<ProductDto>>
{
    private readonly IApplicationDbContext _context;

    public UpdateProductCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<ProductResult<ProductDto>> Handle(UpdateProductCommand command, CancellationToken cancellationToken)
    {
        var product = await _context.SanPhams
            .Include(p => p.DanhMuc)
            .Include(p => p.DonViTinh)
            .FirstOrDefaultAsync(p => p.Id == command.Id, cancellationToken);

        if (product == null)
        {
            return ProductResult<ProductDto>.Failed($"Không tìm thấy sản phẩm với mã #{command.Id}.", 404);
        }

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

        if (req.DanhMucId.HasValue && req.DanhMucId.Value > 0)
        {
            var dmExists = await _context.DanhMucSanPhams.AnyAsync(d => d.Id == req.DanhMucId.Value, cancellationToken);
            if (dmExists)
            {
                product.DanhMucId = req.DanhMucId.Value;
            }
        }
        else if (!string.IsNullOrWhiteSpace(req.TenDanhMuc))
        {
            var trimmedCat = req.TenDanhMuc.Trim();
            var dm = await _context.DanhMucSanPhams
                .FirstOrDefaultAsync(d => d.TenDanhMuc.ToLower() == trimmedCat.ToLower(), cancellationToken);
            if (dm != null)
            {
                product.DanhMucId = dm.Id;
            }
            else
            {
                var newDm = new DanhMucSanPham { TenDanhMuc = trimmedCat, TrangThai = "HoatDong" };
                _context.DanhMucSanPhams.Add(newDm);
                await _context.SaveChangesAsync(cancellationToken);
                product.DanhMucId = newDm.Id;
            }
        }

        if (req.DonViTinhId.HasValue && req.DonViTinhId.Value > 0)
        {
            var dvtExists = await _context.DonViTinhs.AnyAsync(d => d.Id == req.DonViTinhId.Value, cancellationToken);
            if (dvtExists)
            {
                product.DonViTinhId = req.DonViTinhId.Value;
            }
        }
        else if (!string.IsNullOrWhiteSpace(req.TenDonVi))
        {
            var trimmedDvt = req.TenDonVi.Trim();
            var dvt = await _context.DonViTinhs
                .FirstOrDefaultAsync(d => d.TenDonVi.ToLower() == trimmedDvt.ToLower(), cancellationToken);
            if (dvt != null)
            {
                product.DonViTinhId = dvt.Id;
            }
            else
            {
                var newDvt = new DonViTinh { TenDonVi = trimmedDvt };
                _context.DonViTinhs.Add(newDvt);
                await _context.SaveChangesAsync(cancellationToken);
                product.DonViTinhId = newDvt.Id;
            }
        }

        product.TenSP = req.TenSP.Trim();
        product.GiaNhap = req.GiaNhap;
        product.GiaBan = req.GiaBan;
        product.SoLuongTon = req.SoLuongTon;

        if (req.ImageUrl != null)
        {
            product.HinhAnh = string.IsNullOrWhiteSpace(req.ImageUrl) ? null : req.ImageUrl.Trim();
        }

        if (!string.IsNullOrWhiteSpace(req.TrangThai))
        {
            product.TrangThai = req.TrangThai.Trim();
        }

        await _context.SaveChangesAsync(cancellationToken);

        var dmEntity = await _context.DanhMucSanPhams.FindAsync(new object[] { product.DanhMucId }, cancellationToken);
        var dvtEntity = await _context.DonViTinhs.FindAsync(new object[] { product.DonViTinhId }, cancellationToken);

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

        return ProductResult<ProductDto>.Success(dto, "Cập nhật sản phẩm thành công.");
    }
}
