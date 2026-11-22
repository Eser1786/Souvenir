using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Souvenir.Application.DTOs.Products;
using Souvenir.Application.Features.Products.Commands.CreateProduct;
using Souvenir.Application.Features.Products.Commands.DeleteProduct;
using Souvenir.Application.Features.Products.Commands.UpdateProduct;
using Souvenir.Application.Features.Products.Queries.GetMetadata;
using Souvenir.Application.Features.Products.Queries.GetProductById;
using Souvenir.Application.Features.Products.Queries.GetProducts;
using Souvenir.Domain.Constants;

namespace Souvenir.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ProductsController : ControllerBase
{
    private readonly ISender _sender;

    public ProductsController(ISender sender)
    {
        _sender = sender;
    }

    /// <summary>
    /// Lấy metadata sản phẩm: Danh sách danh mục, đơn vị tính và mã gợi ý tiếp theo
    /// </summary>
    [HttpGet("metadata")]
    [ProducesResponseType(typeof(ProductMetadataDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetProductMetadata(CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new GetProductMetadataQuery(), cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Danh sách sản phẩm (hỗ trợ lọc search, category, và includeAllStatus)
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(List<ProductDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetProducts(
        [FromQuery] string? search,
        [FromQuery] string? category,
        [FromQuery] bool includeAllStatus = false,
        CancellationToken cancellationToken = default)
    {
        var query = new GetProductsQuery(search, category, includeAllStatus);
        var products = await _sender.Send(query, cancellationToken);
        return Ok(products);
    }

    /// <summary>
    /// Chi tiết sản phẩm theo ID
    /// </summary>
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(ProductDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetProductById(int id, CancellationToken cancellationToken)
    {
        var product = await _sender.Send(new GetProductByIdQuery(id), cancellationToken);
        if (product == null)
        {
            return NotFound(new { message = $"Không tìm thấy sản phẩm #{id}." });
        }
        return Ok(product);
    }

    /// <summary>
    /// UC11: Thêm sản phẩm mới (nhận imageUrl, tự động map vào HinhAnh)
    /// </summary>
    [HttpPost]
    [Authorize(Roles = VaiTroConstants.QuanLy)]
    [ProducesResponseType(typeof(ProductDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> CreateProduct(
        [FromBody] CreateProductRequest request,
        CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var result = await _sender.Send(new CreateProductCommand(request), cancellationToken);
        if (!result.IsSuccess)
        {
            return StatusCode(result.StatusCode, new { message = result.Message });
        }

        return StatusCode(StatusCodes.Status201Created, result.Data);
    }

    /// <summary>
    /// UC11: Cập nhật thông tin sản phẩm
    /// </summary>
    [HttpPut("{id:int}")]
    [Authorize(Roles = VaiTroConstants.QuanLy)]
    [ProducesResponseType(typeof(ProductDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateProduct(
        int id,
        [FromBody] UpdateProductRequest request,
        CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var result = await _sender.Send(new UpdateProductCommand(id, request), cancellationToken);
        if (!result.IsSuccess)
        {
            return StatusCode(result.StatusCode, new { message = result.Message });
        }

        return Ok(result.Data);
    }

    /// <summary>
    /// UC11: Xóa sản phẩm / Chuyển sang Ngừng kinh doanh nếu đã phát sinh giao dịch
    /// </summary>
    [HttpDelete("{id:int}")]
    [Authorize(Roles = VaiTroConstants.QuanLy)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> DeleteProduct(int id, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new DeleteProductCommand(id), cancellationToken);
        if (!result.IsSuccess)
        {
            return StatusCode(result.StatusCode, new { message = result.Message });
        }

        return Ok(new { message = result.Message, action = result.Data });
    }
}
