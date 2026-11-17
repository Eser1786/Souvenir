using Microsoft.EntityFrameworkCore;
using Souvenir.Application.Interfaces;
using Souvenir.Infrastructure.Data;

namespace Souvenir.Infrastructure.Services;

/// <summary>
/// Đọc thẳng bảng ca_lam_viec bằng SQL vì entity CaLamViec thuộc phân hệ UC03 (Người 5) chưa có.
/// Khi CaLamViec được thêm vào AppDbContext, có thể đổi sang truy vấn LINQ.
/// </summary>
public class CaLamViecLookup : ICaLamViecLookup
{
    private readonly AppDbContext _context;

    public CaLamViecLookup(AppDbContext context)
    {
        _context = context;
    }

    public async Task<int?> GetOpenShiftIdAsync(int nguoiDungId, CancellationToken cancellationToken = default)
    {
        var ids = await _context.Database
            .SqlQuery<int>($"SELECT id AS \"Value\" FROM ca_lam_viec WHERE nguoidungid = {nguoiDungId} AND thoigianchotca IS NULL ORDER BY thoigianmoca DESC, id DESC LIMIT 1")
            .ToListAsync(cancellationToken);
        return ids.Count > 0 ? ids[0] : null;
    }

    public async Task<bool> IsShiftOpenAsync(int caLamViecId, CancellationToken cancellationToken = default)
    {
        var ids = await _context.Database
            .SqlQuery<int>($"SELECT id AS \"Value\" FROM ca_lam_viec WHERE id = {caLamViecId} AND thoigianchotca IS NULL")
            .ToListAsync(cancellationToken);
        return ids.Count > 0;
    }
}
