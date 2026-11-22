using System.Security.Claims;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Souvenir.Application.DTOs.StockAudits;
using Souvenir.Application.Features.StockAudits.Commands.BalanceStockAudit;
using Souvenir.Application.Features.StockAudits.Commands.SaveDraftStockAudit;
using Souvenir.Application.Features.StockAudits.Queries.GetStockAuditById;
using Souvenir.Application.Features.StockAudits.Queries.GetStockAudits;
using Souvenir.Domain.Constants;

namespace Souvenir.Api.Controllers;

/// <summary>UC13 - Kiểm kê kho và cân bằng tồn kho thực tế (chỉ Quản lý).</summary>
[ApiController]
[Route("api/stock-audits")]
[Authorize(Roles = VaiTroConstants.QuanLy)]
public class StockAuditsController : ControllerBase
{
    private readonly ISender _sender;

    public StockAuditsController(ISender sender)
    {
        _sender = sender;
    }

    private bool TryGetUserId(out int userId)
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? User.FindFirst("sub")?.Value;
        return int.TryParse(userIdClaim, out userId);
    }

    /// <summary>Lấy danh sách phiếu kiểm kê kho (lọc theo trạng thái DangKiemKe | DaCanBang nếu truyền).</summary>
    [HttpGet]
    [ProducesResponseType(typeof(List<StockAuditDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetStockAudits(
        [FromQuery] string? trangThai,
        CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new GetStockAuditsQuery(trangThai), cancellationToken);
        return Ok(result);
    }

    /// <summary>Xem chi tiết một phiếu kiểm kê kho.</summary>
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(StockAuditDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetStockAuditById(int id, CancellationToken cancellationToken)
    {
        var audit = await _sender.Send(new GetStockAuditByIdQuery(id), cancellationToken);
        return audit == null ? NotFound(new { message = "Không tìm thấy phiếu kiểm kê." }) : Ok(audit);
    }

    /// <summary>Lưu tạm kết quả kiểm kê (trạng thái DangKiemKe, chưa thay đổi tồn kho).</summary>
    [HttpPost("draft")]
    [ProducesResponseType(typeof(StockAuditDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(StockAuditDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> SaveDraft(
        [FromBody] SaveStockAuditRequest request,
        CancellationToken cancellationToken)
    {
        if (!TryGetUserId(out var userId))
        {
            return Unauthorized(new { message = "Phiên đăng nhập không hợp lệ." });
        }

        var result = await _sender.Send(new SaveDraftStockAuditCommand(userId, request), cancellationToken);
        if (!result.IsSuccess)
        {
            return StatusCode(result.StatusCode, new { message = result.Message });
        }

        return StatusCode(result.StatusCode, result.Data);
    }

    /// <summary>Xác nhận cân bằng tồn kho (bắt buộc nhập lý do nếu có chênh lệch, cập nhật tồn kho và khóa phiếu).</summary>
    [HttpPost("balance")]
    [ProducesResponseType(typeof(StockAuditDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(StockAuditDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Balance(
        [FromBody] SaveStockAuditRequest request,
        CancellationToken cancellationToken)
    {
        if (!TryGetUserId(out var userId))
        {
            return Unauthorized(new { message = "Phiên đăng nhập không hợp lệ." });
        }

        var result = await _sender.Send(new BalanceStockAuditCommand(userId, request), cancellationToken);
        if (!result.IsSuccess)
        {
            return StatusCode(result.StatusCode, new { message = result.Message });
        }

        return StatusCode(result.StatusCode, result.Data);
    }
}
