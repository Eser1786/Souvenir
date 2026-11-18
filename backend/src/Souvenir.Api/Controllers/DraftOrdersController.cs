using System.Security.Claims;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Souvenir.Application.DTOs.Orders;
using Souvenir.Application.Features.DraftOrders.Commands.CreateDraftOrder;
using Souvenir.Application.Features.DraftOrders.Commands.DeleteDraftOrder;
using Souvenir.Application.Features.DraftOrders.Queries.GetDraftOrderById;
using Souvenir.Application.Features.DraftOrders.Queries.GetDraftOrders;

namespace Souvenir.Api.Controllers;

/// <summary>
/// UC06 - Quản lý đơn nháp (lưu tạm giỏ hàng và khôi phục).
/// </summary>
[ApiController]
[Route("api/draft-orders")]
[Authorize]
public class DraftOrdersController : ControllerBase
{
    private readonly ISender _sender;

    public DraftOrdersController(ISender sender)
    {
        _sender = sender;
    }

    /// <summary>
    /// Lưu nháp đơn hàng hiện tại (không trừ kho).
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(DraftOrderDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> CreateDraftOrder([FromBody] CreateDraftOrderRequest request, CancellationToken cancellationToken)
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? User.FindFirst("sub")?.Value;
        if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out var userId))
        {
            return Unauthorized(new { message = "Phiên đăng nhập không hợp lệ." });
        }

        var result = await _sender.Send(new CreateDraftOrderCommand(userId, request), cancellationToken);
        if (!result.IsSuccess)
        {
            return StatusCode(result.StatusCode, new { message = result.Message });
        }

        return StatusCode(StatusCodes.Status201Created, result.Data);
    }

    /// <summary>
    /// Lấy danh sách các đơn đang lưu nháp.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(List<DraftOrderDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetDraftOrders(CancellationToken cancellationToken)
    {
        var drafts = await _sender.Send(new GetDraftOrdersQuery(), cancellationToken);
        return Ok(drafts);
    }

    /// <summary>
    /// Xem chi tiết một đơn hàng nháp.
    /// </summary>
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(DraftOrderDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetDraftOrderById(int id, CancellationToken cancellationToken)
    {
        var draft = await _sender.Send(new GetDraftOrderByIdQuery(id), cancellationToken);
        return draft == null ? NotFound(new { message = "Không tìm thấy đơn nháp." }) : Ok(draft);
    }

    /// <summary>
    /// Xóa một đơn nháp (khi đã khôi phục hoặc khách hủy).
    /// </summary>
    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteDraftOrder(int id, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new DeleteDraftOrderCommand(id), cancellationToken);
        if (!result.IsSuccess)
        {
            return StatusCode(result.StatusCode, new { message = result.Message });
        }

        return Ok(new { message = result.Message });
    }
}
