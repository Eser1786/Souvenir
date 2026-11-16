using Microsoft.EntityFrameworkCore;
using Souvenir.Domain.Entities;

namespace Souvenir.Application.Interfaces;

public interface IApplicationDbContext
{
    DbSet<NguoiDung> NguoiDungs { get; }
    DbSet<VaiTro> VaiTros { get; }
    DbSet<ThamSo> ThamSos { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
