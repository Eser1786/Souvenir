using MediatR;
using Souvenir.Application.DTOs.Products;

namespace Souvenir.Application.Features.Products.Commands.UpdateProduct;

public record UpdateProductCommand(int Id, UpdateProductRequest Request) : IRequest<ProductResult<ProductDto>>;
