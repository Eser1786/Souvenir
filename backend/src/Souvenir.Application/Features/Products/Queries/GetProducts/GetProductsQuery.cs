using MediatR;
using Souvenir.Application.DTOs.Products;

namespace Souvenir.Application.Features.Products.Queries.GetProducts;

public record GetProductsQuery(string? Search, string? Category) : IRequest<List<ProductDto>>;
