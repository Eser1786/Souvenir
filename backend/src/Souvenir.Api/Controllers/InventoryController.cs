using System.Security.Claims;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Souvenir.Application.DTOs.Inventory;
using Souvenir.Application.Features.Inventory.Commands.CreateGoodsReceipt;
using Souvenir.Application.Features.Inventory.Queries.GetGoodsReceipts;
using Souvenir.Application.Features.Inventory.Queries.GetSuppliers;

namespace Souvenir.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class InventoryController : ControllerBase
{
    private readonly ISender _sender;

    public InventoryController(ISender sender)
    {
        _sender = sender;
    }

    /// <summary>
    /// Lấy danh sách các nhà cung cấp
    /// </summary>
    [HttpGet("suppliers")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(List<SupplierDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetSuppliers(CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new GetSuppliersQuery(), cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Lấy danh sách các phiếu nhập kho đã lập
    /// </summary>
    [HttpGet("receipts")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(List<GoodsReceiptDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetGoodsReceipts(CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new GetGoodsReceiptsQuery(), cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// UC12: Lập phiếu nhập kho và tự động cộng dồn số lượng tồn kho (soLuongTon)
    /// </summary>
    [HttpPost("receipts")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(GoodsReceiptDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> CreateGoodsReceipt(
        [FromBody] CreateGoodsReceiptRequest request,
        CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? User.FindFirst("sub")?.Value;
        int userId = 1; // Default Manager
        if (!string.IsNullOrEmpty(userIdClaim) && int.TryParse(userIdClaim, out var parsedId))
        {
            userId = parsedId;
        }

        var result = await _sender.Send(new CreateGoodsReceiptCommand(userId, request), cancellationToken);
        if (!result.IsSuccess)
        {
            return StatusCode(result.StatusCode, new { message = result.Message });
        }

        return StatusCode(StatusCodes.Status201Created, result.Data);
    }
}
