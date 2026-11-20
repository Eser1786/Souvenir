using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Souvenir.Application.DTOs.Customers;
using Souvenir.Application.Features.Customers.Commands.CreateCustomer;
using Souvenir.Application.Features.Customers.Commands.DeleteCustomer;
using Souvenir.Application.Features.Customers.Commands.UpdateCustomer;
using Souvenir.Application.Features.Customers.Queries.GetCustomerById;
using Souvenir.Application.Features.Customers.Queries.GetCustomerOrders;
using Souvenir.Application.Features.Customers.Queries.GetCustomers;

namespace Souvenir.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class CustomersController : ControllerBase
{
    private readonly ISender _sender;

    public CustomersController(ISender sender)
    {
        _sender = sender;
    }

    /// <summary>
    /// Tra cứu danh sách khách hàng hoặc tìm kiếm theo Số điện thoại / Họ tên
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(List<CustomerDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetCustomers(
        [FromQuery] string? search,
        [FromQuery] string? phone,
        CancellationToken cancellationToken)
    {
        var query = new GetCustomersQuery(search, phone);
        var result = await _sender.Send(query, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Lấy chi tiết hồ sơ khách hàng theo ID
    /// </summary>
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(CustomerDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetCustomerById(int id, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new GetCustomerByIdQuery(id), cancellationToken);
        if (result == null)
        {
            return NotFound(new { message = $"Không tìm thấy khách hàng với mã #{id}." });
        }

        return Ok(result);
    }

    /// <summary>
    /// Lấy danh sách lịch sử đơn hàng của một khách hàng (UC09)
    /// </summary>
    [HttpGet("{id:int}/orders")]
    [ProducesResponseType(typeof(List<CustomerOrderDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetCustomerOrders(int id, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new GetCustomerOrdersQuery(id), cancellationToken);
        if (!result.IsSuccess)
        {
            return StatusCode(result.StatusCode, new { message = result.Message });
        }

        return Ok(result.Data);
    }

    /// <summary>
    /// Tạo mới hồ sơ khách hàng thân thiết
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(CustomerDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> CreateCustomer(
        [FromBody] CreateCustomerRequest request,
        CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var result = await _sender.Send(
            new CreateCustomerCommand(request.SoDienThoai, request.TenKhachHang),
            cancellationToken);

        if (!result.IsSuccess)
        {
            return StatusCode(result.StatusCode, new { message = result.Message });
        }

        return StatusCode(StatusCodes.Status201Created, result.Data);
    }

    /// <summary>
    /// Cập nhật thông tin và điểm tích lũy của khách hàng
    /// </summary>
    [HttpPut("{id:int}")]
    [ProducesResponseType(typeof(CustomerDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> UpdateCustomer(
        int id,
        [FromBody] UpdateCustomerRequest request,
        CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var result = await _sender.Send(
            new UpdateCustomerCommand(id, request.SoDienThoai, request.TenKhachHang, request.DiemTichLuy),
            cancellationToken);

        if (!result.IsSuccess)
        {
            return StatusCode(result.StatusCode, new { message = result.Message });
        }

        return Ok(result.Data);
    }

    /// <summary>
    /// Xóa hồ sơ khách hàng
    /// </summary>
    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> DeleteCustomer(int id, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new DeleteCustomerCommand(id), cancellationToken);

        if (!result.IsSuccess)
        {
            return StatusCode(result.StatusCode, new { message = result.Message });
        }

        return Ok(new { message = result.Message });
    }
}
