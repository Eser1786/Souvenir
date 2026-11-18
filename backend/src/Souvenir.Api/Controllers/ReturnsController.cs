using System.Security.Claims;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Souvenir.Application.DTOs.Returns;
using Souvenir.Application.Features.Returns.Commands.CreateReturn;
using Souvenir.Application.Features.Returns.Queries.GetReturnableOrder;
using Souvenir.Application.Features.Returns.Queries.GetReturns;
using Souvenir.Application.Features.Returns.Queries.SearchReturnableOrders;
using Souvenir.Domain.Constants;

namespace Souvenir.Api.Controllers;

/// <summary>UC08 - Trả hàng / Hoàn tiền (chỉ Quản lý).</summary>
[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = VaiTroConstants.QuanLy)]
public class ReturnsController : ControllerBase
{
    private readonly ISender _sender;

    public ReturnsController(ISender sender)
    {
        _sender = sender;
    }

    /// <summary>Tra cứu hóa đơn gốc để trả hàng.</summary>
    /// <param name="keyword">Mã đơn (DH261117-00012 hoặc 12) hoặc số điện thoại khách.</param>
    /// <param name="tuNgay">Ngày mua từ (yyyy-MM-dd).</param>
    /// <param name="denNgay">Ngày mua đến (yyyy-MM-dd).</param>
    /// <param name="cancellationToken">Cancellation token</param>
    [HttpGet("orders")]
    [ProducesResponseType(typeof(List<ReturnableOrderDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> SearchOrders(
        [FromQuery] string? keyword,
        [FromQuery] DateTime? tuNgay,
        [FromQuery] DateTime? denNgay,
        CancellationToken cancellationToken)
    {
        return Ok(await _sender.Send(new SearchReturnableOrdersQuery(keyword, tuNgay, denNgay), cancellationToken));
    }

    /// <summary>Chi tiết hóa đơn gốc kèm số lượng còn được trả từng sản phẩm.</summary>
    [HttpGet("orders/{id:int}")]
    [ProducesResponseType(typeof(ReturnableOrderDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetOrder(int id, CancellationToken cancellationToken)
    {
        var order = await _sender.Send(new GetReturnableOrderQuery(id), cancellationToken);
        return order == null ? NotFound(new { message = "Không tìm thấy hóa đơn." }) : Ok(order);
    }

    /// <summary>
    /// Lập phiếu trả hàng: kiểm tra hạn đổi trả (SoNgayChoPhepDoiTra), đổi đơn sang DaTraHang, cộng trả tồn kho.
    /// </summary>
    /// <remarks>
    /// Ví dụ body:
    ///
    ///     {
    ///       "maDonHang": "DH261118-00002",
    ///       "lyDoTra": "Khách đổi ý, hàng còn nguyên tem",
    ///       "items": [ { "sanPhamId": 1, "soLuongTra": 1, "tinhTrangHang": "NguyenVen" } ]
    ///     }
    ///
    /// tinhTrangHang: "NguyenVen" | "LoiNhaSanXuat" | "HuHongDoKhach" (bị từ chối).
    /// </remarks>
    [HttpPost]
    [ProducesResponseType(typeof(ReturnDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> CreateReturn([FromBody] CreateReturnRequest request, CancellationToken cancellationToken)
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? User.FindFirst("sub")?.Value;
        if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out var userId))
        {
            return Unauthorized(new { message = "Phiên đăng nhập không hợp lệ." });
        }

        var result = await _sender.Send(new CreateReturnCommand(userId, request), cancellationToken);
        if (!result.IsSuccess)
        {
            return StatusCode(result.StatusCode, new { message = result.Message });
        }

        return CreatedAtAction(nameof(GetReturnById), new { id = result.Data!.Id }, result.Data);
    }

    /// <summary>Danh sách phiếu trả (lọc theo đơn gốc nếu truyền donHangId).</summary>
    [HttpGet]
    [ProducesResponseType(typeof(List<ReturnDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetReturns([FromQuery] int? donHangId, CancellationToken cancellationToken)
    {
        return Ok(await _sender.Send(new GetReturnsQuery(null, donHangId), cancellationToken));
    }

    /// <summary>Chi tiết một phiếu trả.</summary>
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(ReturnDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetReturnById(int id, CancellationToken cancellationToken)
    {
        var list = await _sender.Send(new GetReturnsQuery(id), cancellationToken);
        return list.Count == 0 ? NotFound(new { message = "Không tìm thấy phiếu trả." }) : Ok(list[0]);
    }
}
