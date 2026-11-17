using Souvenir.Application.Shifts.DTOs;

namespace Souvenir.Application.Interfaces;

public interface IShiftService
{
    Task<ShiftResponse?> GetCurrentShiftAsync(int userId);
    Task<ShiftResponse> OpenShiftAsync(int userId, OpenShiftRequest request);
    Task<ShiftResponse> CloseShiftAsync(int userId, int shiftId, CloseShiftRequest request);
    Task<ShiftResponse> ExplainDiscrepancyAsync(int shiftId, ExplainShiftRequest request);
    Task<AttendanceResponse> CheckInAsync(int userId);
    Task<AttendanceResponse> CheckOutAsync(int userId);
}