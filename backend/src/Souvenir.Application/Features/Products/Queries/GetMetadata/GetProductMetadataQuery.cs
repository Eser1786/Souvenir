using MediatR;
using Microsoft.EntityFrameworkCore;
using Souvenir.Application.Common;
using Souvenir.Application.DTOs.Products;
using Souvenir.Application.Interfaces;
using Souvenir.Domain.Entities;

namespace Souvenir.Application.Features.Products.Queries.GetMetadata;

public class CategoryDto
{
    public int Id { get; set; }
    public string TenDanhMuc { get; set; } = string.Empty;
}

public class UnitDto
{
    public int Id { get; set; }
    public string TenDonVi { get; set; } = string.Empty;
}

public class ProductMetadataDto
{
    public List<CategoryDto> Categories { get; set; } = new();
    public List<UnitDto> Units { get; set; } = new();
    public string NextSuggestedMaSP { get; set; } = "SP001";
}

public record GetProductMetadataQuery : IRequest<ProductMetadataDto>;

public class GetProductMetadataQueryHandler : IRequestHandler<GetProductMetadataQuery, ProductMetadataDto>
{
    private readonly IApplicationDbContext _context;

    public GetProductMetadataQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<ProductMetadataDto> Handle(GetProductMetadataQuery request, CancellationToken cancellationToken)
    {
        var categories = await _context.DanhMucSanPhams
            .AsNoTracking()
            .OrderBy(c => c.Id)
            .Select(c => new CategoryDto { Id = c.Id, TenDanhMuc = c.TenDanhMuc })
            .ToListAsync(cancellationToken);

        var units = await _context.DonViTinhs
            .AsNoTracking()
            .OrderBy(u => u.Id)
            .Select(u => new UnitDto { Id = u.Id, TenDonVi = u.TenDonVi })
            .ToListAsync(cancellationToken);

        // Sinh mã SP tiếp theo theo quy chuẩn SPxxx
        var existingMaSPs = await _context.SanPhams
            .AsNoTracking()
            .Select(p => p.MaSP)
            .ToListAsync(cancellationToken);

        int maxNum = 0;
        foreach (var ma in existingMaSPs)
        {
            if (string.IsNullOrWhiteSpace(ma)) continue;
            var clean = ma.Trim().ToUpper();
            if (clean.StartsWith("SP") && int.TryParse(clean[2..], out var num))
            {
                if (num > maxNum) maxNum = num;
            }
        }

        string nextMa = $"SP{(maxNum + 1):D3}";

        return new ProductMetadataDto
        {
            Categories = categories,
            Units = units,
            NextSuggestedMaSP = nextMa
        };
    }
}
