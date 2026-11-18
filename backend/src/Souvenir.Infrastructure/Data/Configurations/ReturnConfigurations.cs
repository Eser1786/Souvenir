using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Souvenir.Domain.Entities;

namespace Souvenir.Infrastructure.Data.Configurations;

// UC08 - Ánh xạ khớp với bảng PHIEU_TRA_HANG, CHI_TIET_TRA_HANG trong database/init/01_init.sql.

public class PhieuTraHangConfiguration : IEntityTypeConfiguration<PhieuTraHang>
{
    public void Configure(EntityTypeBuilder<PhieuTraHang> entity)
    {
        entity.ToTable("phieu_tra_hang");
        entity.HasKey(e => e.Id);
        entity.Property(e => e.Id).HasColumnName("id").ValueGeneratedOnAdd();
        entity.Property(e => e.MaPhieu).HasColumnName("maphieu").HasMaxLength(50).IsRequired();
        entity.Property(e => e.DonHangGocId).HasColumnName("donhanggocid").IsRequired();
        entity.Property(e => e.NguoiDungId).HasColumnName("nguoidungid").IsRequired();
        entity.Property(e => e.TongTienHoan).HasColumnName("tongtienhoan").HasColumnType("numeric(18,2)").IsRequired();
        entity.Property(e => e.LyDoTra).HasColumnName("lydotra").HasMaxLength(255).IsRequired();
        entity.Property(e => e.NgayTra).HasColumnName("ngaytra").HasColumnType("timestamp without time zone").IsRequired();

        entity.HasOne(e => e.DonHangGoc)
            .WithMany()
            .HasForeignKey(e => e.DonHangGocId)
            .HasConstraintName("fk_pth_donhang");

        entity.HasOne(e => e.NguoiDung)
            .WithMany()
            .HasForeignKey(e => e.NguoiDungId)
            .HasConstraintName("fk_pth_nguoidung");
    }
}

public class ChiTietTraHangConfiguration : IEntityTypeConfiguration<ChiTietTraHang>
{
    public void Configure(EntityTypeBuilder<ChiTietTraHang> entity)
    {
        entity.ToTable("chi_tiet_tra_hang");
        entity.HasKey(e => new { e.PhieuTraHangId, e.SanPhamId });
        entity.Property(e => e.PhieuTraHangId).HasColumnName("phieutrahangid");
        entity.Property(e => e.SanPhamId).HasColumnName("sanphamid");
        entity.Property(e => e.SoLuongTra).HasColumnName("soluongtra").IsRequired();
        entity.Property(e => e.DonGiaHoan).HasColumnName("dongiahoan").HasColumnType("numeric(18,2)").IsRequired();
        entity.Property(e => e.TinhTrangHang).HasColumnName("tinhtranghang").HasMaxLength(255).IsRequired();

        entity.HasOne(e => e.PhieuTraHang)
            .WithMany(p => p.ChiTietTraHangs)
            .HasForeignKey(e => e.PhieuTraHangId)
            .HasConstraintName("fk_ctth_phieutrahang");

        entity.HasOne(e => e.SanPham)
            .WithMany()
            .HasForeignKey(e => e.SanPhamId)
            .HasConstraintName("fk_ctth_sanpham");
    }
}
