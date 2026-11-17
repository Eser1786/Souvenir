using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Souvenir.Application.Shifts.DTOs;
using Souvenir.Application.Interfaces;

namespace Souvenir.Api.Controllers;

[ApiController]
[Route("api")]
[Authorize]
public class ShiftsController : ControllerBase
{
    private readonly IShiftService _shiftService;

    public ShiftsController(IShiftService shiftService) => _shiftService = shiftService;

    private int GetUserId()
    {
        var idStr = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub");
        return int.Parse(idStr!);
    }

    [HttpGet("shifts/current")]
    public async Task<IActionResult> GetCurrent()
    {
        var data = await _shiftService.GetCurrentShiftAsync(GetUserId());
        return Ok(new { success = true, data });
    }

    [HttpPost("shifts/open")]
    public async Task<IActionResult> OpenShift([FromBody] OpenShiftRequest request)
    {
        var data = await _shiftService.OpenShiftAsync(GetUserId(), request);
        return Ok(new { success = true, data, message = "Mở ca thành công" });
    }

    [HttpPost("shifts/{id:int}/close")]
    public async Task<IActionResult> CloseShift(int id, [FromBody] CloseShiftRequest request)
    {
        var data = await _shiftService.CloseShiftAsync(GetUserId(), id, request);
        return Ok(new { success = true, data, message = "Đã ghi nhận kiểm đếm chốt ca" });
    }

    [HttpPost("shifts/{id:int}/explain")]
    public async Task<IActionResult> Explain(int id, [FromBody] ExplainShiftRequest request)
    {
        var data = await _shiftService.ExplainDiscrepancyAsync(id, request);
        return Ok(new { success = true, data, message = "Đã giải trình và chốt ca" });
    }

    [HttpPost("attendance/check-in")]
    public async Task<IActionResult> CheckIn()
    {
        var data = await _shiftService.CheckInAsync(GetUserId());
        return Ok(new { success = true, data, message = "Check-in thành công" });
    }

    [HttpPost("attendance/check-out")]
    public async Task<IActionResult> CheckOut()
    {
        var data = await _shiftService.CheckOutAsync(GetUserId());
        return Ok(new { success = true, data, message = "Check-out thành công" });
    }
}