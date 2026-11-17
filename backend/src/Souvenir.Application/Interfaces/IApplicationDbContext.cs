using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Souvenir.Domain.Entities;

namespace Souvenir.Application.Interfaces;

public interface IApplicationDbContext
{
    DbSet<NguoiDung> NguoiDungs { get; }
    DbSet<VaiTro> VaiTros { get; }
    DbSet<ThamSo> ThamSos { get; }
    DbSet<SanPham> SanPhams { get; }
    DbSet<DanhMucSanPham> DanhMucSanPhams { get; }
    DbSet<DonViTinh> DonViTinhs { get; }
    DbSet<KhachHang> KhachHangs { get; }
    DbSet<MaGiamGia> MaGiamGias { get; }
    DbSet<HinhThucThanhToan> HinhThucThanhToans { get; }
    DbSet<DonHang> DonHangs { get; }
    DbSet<ChiTietDonHang> ChiTietDonHangs { get; }
    DbSet<CaLamViec> CaLamViecs { get; }
    DbSet<ChamCong> ChamCongs { get; }

    /// <summary>Dùng để mở transaction (UC07 tạo đơn + trừ kho).</summary>
    DatabaseFacade Database { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
