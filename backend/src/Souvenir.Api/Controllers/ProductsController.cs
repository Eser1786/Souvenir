using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Souvenir.Application.DTOs.Products;
using Souvenir.Application.Features.Products.Queries.GetProducts;

namespace Souvenir.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProductsController : ControllerBase
{
    private readonly ISender _sender;

    public ProductsController(ISender sender)
    {
        _sender = sender;
    }

    [HttpGet]
    [AllowAnonymous]
    [ProducesResponseType(typeof(List<ProductDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetProducts(
        [FromQuery] string? search,
        [FromQuery] string? category,
        CancellationToken cancellationToken)
    {
        var query = new GetProductsQuery(search, category);
        var products = await _sender.Send(query, cancellationToken);
        return Ok(products);
    }
}
