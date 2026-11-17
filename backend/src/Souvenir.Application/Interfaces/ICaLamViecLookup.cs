namespace Souvenir.Application.Interfaces;

/// <summary>
/// Tra cứu ca làm việc mà không phụ thuộc vào entity CaLamViec (phân hệ UC03 của Người 5).
/// Khi entity CaLamViec được thêm vào, chỉ cần thay phần cài đặt ở Infrastructure.
/// </summary>
public interface ICaLamViecLookup
{
    /// <summary>Id ca đang mở (chưa chốt) gần nhất của nhân viên, hoặc null.</summary>
    Task<int?> GetOpenShiftIdAsync(int nguoiDungId, CancellationToken cancellationToken = default);

    /// <summary>Ca có tồn tại và còn đang mở hay không.</summary>
    Task<bool> IsShiftOpenAsync(int caLamViecId, CancellationToken cancellationToken = default);
}
