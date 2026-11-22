using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Souvenir.Domain.Entities;

namespace Souvenir.Infrastructure.Data.Configurations;

// UC13 - Ánh xạ khớp với bảng PHIEU_KIEM_KE, CHI_TIET_KIEM_KE trong database/init/01_init.sql.

public class PhieuKiemKeConfiguration : IEntityTypeConfiguration<PhieuKiemKe>
{
    public void Configure(EntityTypeBuilder<PhieuKiemKe> entity)
    {
        entity.ToTable("phieu_kiem_ke");
        entity.HasKey(e => e.Id);
        entity.Property(e => e.Id).HasColumnName("id").ValueGeneratedOnAdd();
        entity.Property(e => e.MaPhieu).HasColumnName("maphieu").HasMaxLength(50).IsRequired();
        entity.HasIndex(e => e.MaPhieu).IsUnique();
        entity.Property(e => e.NguoiDungId).HasColumnName("nguoidungid").IsRequired();
        entity.Property(e => e.NgayKiemKe).HasColumnName("ngaykiemke").HasColumnType("timestamp without time zone").IsRequired();
        entity.Property(e => e.TrangThai).HasColumnName("trangthai").HasMaxLength(50).HasDefaultValue(TrangThaiPhieuKiemKe.DangKiemKe).IsRequired();

        entity.HasOne(e => e.NguoiDung)
            .WithMany()
            .HasForeignKey(e => e.NguoiDungId)
            .HasConstraintName("fk_pkk_nguoidung");
    }
}

public class ChiTietKiemKeConfiguration : IEntityTypeConfiguration<ChiTietKiemKe>
{
    public void Configure(EntityTypeBuilder<ChiTietKiemKe> entity)
    {
        entity.ToTable("chi_tiet_kiem_ke");
        entity.HasKey(e => new { e.PhieuKiemKeId, e.SanPhamId });
        entity.Property(e => e.PhieuKiemKeId).HasColumnName("phieukiemkeid");
        entity.Property(e => e.SanPhamId).HasColumnName("sanphamid");
        entity.Property(e => e.SoTonSoSach).HasColumnName("sotonsosach").IsRequired();
        entity.Property(e => e.SoTonThucTe).HasColumnName("sotonthucte").IsRequired();
        entity.Property(e => e.SoLuongChenhLech).HasColumnName("soluongchenhlech").IsRequired();
        entity.Property(e => e.LyDoChenhLech).HasColumnName("lydochenhlech").HasMaxLength(255);

        entity.HasOne(e => e.PhieuKiemKe)
            .WithMany(p => p.ChiTietKiemKes)
            .HasForeignKey(e => e.PhieuKiemKeId)
            .HasConstraintName("fk_ctkk_phieukiemke");

        entity.HasOne(e => e.SanPham)
            .WithMany()
            .HasForeignKey(e => e.SanPhamId)
            .HasConstraintName("fk_ctkk_sanpham");
    }
}
