using MediatR;
using Souvenir.Application.DTOs.Products;

namespace Souvenir.Application.Features.Products.Commands.CreateProduct;

public record CreateProductCommand(CreateProductRequest Request) : IRequest<ProductResult<ProductDto>>;
