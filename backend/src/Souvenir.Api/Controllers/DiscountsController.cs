using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Souvenir.Application.DTOs.Discounts;
using Souvenir.Application.Features.Discounts.Queries.CheckDiscount;

namespace Souvenir.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class DiscountsController : ControllerBase
{
    private readonly ISender _sender;

    public DiscountsController(ISender sender)
    {
        _sender = sender;
    }

    /// <summary>
    /// Kiểm tra tính hợp lệ của mã giảm giá và tính số tiền giảm giá
    /// </summary>
    /// <param name="request">Thông tin mã code và tổng tiền hàng hiện tại</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Thông tin hợp lệ, phần trăm/số tiền giảm, và tổng thanh toán mới</returns>
    [HttpPost("check")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(CheckDiscountResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(CheckDiscountResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CheckDiscount(
        [FromBody] CheckDiscountRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new CheckDiscountQuery(request.MaCode, request.TongTienHang), cancellationToken);

        if (!result.HopLe)
        {
            return BadRequest(result);
        }

        return Ok(result);
    }
}
