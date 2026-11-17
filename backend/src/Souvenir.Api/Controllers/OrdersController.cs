using System.Security.Claims;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Souvenir.Application.DTOs.Orders;
using Souvenir.Application.Features.Orders.Commands.CreateOrder;
using Souvenir.Application.Features.Orders.Queries.GetOrderById;
using Souvenir.Application.Features.Orders.Queries.GetPaymentMethods;

namespace Souvenir.Api.Controllers;

/// <summary>UC07 - Tạo đơn hàng & thanh toán tại quầy POS.</summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class OrdersController : ControllerBase
{
    private readonly ISender _sender;

    public OrdersController(ISender sender)
    {
        _sender = sender;
    }

    /// <summary>
    /// Tạo đơn hàng đã thanh toán: lưu DonHang → lưu ChiTietDonHang → trừ SanPham.SoLuongTon (một transaction).
    /// </summary>
    /// <remarks>
    /// Ví dụ body (tiền mặt):
    ///
    ///     {
    ///       "items": [ { "sanPhamId": 1, "soLuong": 2 }, { "sanPhamId": 3, "soLuong": 1 } ],
    ///       "phuongThuc": "TienMat",
    ///       "tienKhachDua": 500000,
    ///       "maGiamGia": "QUATANG10"
    ///     }
    ///
    /// `phuongThuc`: "TienMat" | "ChuyenKhoan". `caLamViecId` để trống thì lấy ca đang mở của nhân viên đăng nhập.
    /// </remarks>
    [HttpPost]
    [ProducesResponseType(typeof(OrderDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> CreateOrder([FromBody] CreateOrderRequest request, CancellationToken cancellationToken)
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? User.FindFirst("sub")?.Value;
        if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out var userId))
        {
            return Unauthorized(new { message = "Phiên đăng nhập không hợp lệ." });
        }

        var result = await _sender.Send(new CreateOrderCommand(userId, request), cancellationToken);
        if (!result.IsSuccess)
        {
            return StatusCode(result.StatusCode, new { message = result.Message });
        }

        return CreatedAtAction(nameof(GetOrderById), new { id = result.Data!.Id }, result.Data);
    }

    /// <summary>Xem chi tiết một đơn hàng.</summary>
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(OrderDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetOrderById(int id, CancellationToken cancellationToken)
    {
        var order = await _sender.Send(new GetOrderByIdQuery(id), cancellationToken);
        return order == null ? NotFound(new { message = "Không tìm thấy đơn hàng." }) : Ok(order);
    }

    /// <summary>Danh sách hình thức thanh toán (bảng hinh_thuc_thanh_toan).</summary>
    [HttpGet("payment-methods")]
    [ProducesResponseType(typeof(List<PaymentMethodDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetPaymentMethods(CancellationToken cancellationToken)
    {
        return Ok(await _sender.Send(new GetPaymentMethodsQuery(), cancellationToken));
    }
}
