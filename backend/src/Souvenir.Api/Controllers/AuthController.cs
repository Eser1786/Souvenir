using System.Security.Claims;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Souvenir.Application.DTOs.Auth;
using Souvenir.Application.Features.Auth.Commands.Login;
using Souvenir.Application.Features.Auth.Queries.GetCurrentUser;

namespace Souvenir.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly ISender _sender;

    public AuthController(ISender sender)
    {
        _sender = sender;
    }

    [HttpPost("login")]
    [ProducesResponseType(typeof(LoginResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> Login([FromBody] LoginRequest request, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new LoginCommand(request.TenDangNhap, request.MatKhau), cancellationToken);

        if (result.IsLocked)
        {
            return StatusCode(StatusCodes.Status403Forbidden, new
            {
                message = result.Message,
                isLocked = true
            });
        }

        if (!result.IsSuccess)
        {
            return StatusCode(result.StatusCode, new
            {
                message = result.Message,
                remainingAttempts = result.RemainingAttempts,
                isLocked = result.IsLocked
            });
        }

        return Ok(result.Data);
    }

    [HttpGet("me")]
    [Authorize]
    [ProducesResponseType(typeof(UserDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetCurrentUser(CancellationToken cancellationToken)
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
            ?? User.FindFirst("sub")?.Value;

        if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out var userId))
        {
            return Unauthorized(new { message = "Thông tin xác thực không hợp lệ." });
        }

        var user = await _sender.Send(new GetCurrentUserQuery(userId), cancellationToken);
        if (user == null)
        {
            return NotFound(new { message = "Không tìm thấy thông tin người dùng." });
        }

        return Ok(user);
    }

    [HttpPost("logout")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public IActionResult Logout()
    {
        return Ok(new { message = "Đăng xuất thành công." });
    }
}
