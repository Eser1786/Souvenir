using MediatR;
using Souvenir.Application.DTOs.Products;

namespace Souvenir.Application.Features.Products.Commands.DeleteProduct;

public record DeleteProductCommand(int Id) : IRequest<ProductResult<string>>;
