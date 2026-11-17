using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Souvenir.Domain.Entities;

namespace Souvenir.Infrastructure.Data.Configurations;

// Ánh xạ khớp với database/init/01_init.sql (bảng DON_HANG, CHI_TIET_DON_HANG, HINH_THUC_THANH_TOAN).
// ca_lam_viec / khach_hang chưa có entity nên chỉ map cột Id, chưa khai báo navigation.

public class HinhThucThanhToanConfiguration : IEntityTypeConfiguration<HinhThucThanhToan>
{
    public void Configure(EntityTypeBuilder<HinhThucThanhToan> entity)
    {
        entity.ToTable("hinh_thuc_thanh_toan");
        entity.HasKey(e => e.Id);
        entity.Property(e => e.Id).HasColumnName("id").ValueGeneratedOnAdd();
        entity.Property(e => e.TenHinhThuc).HasColumnName("tenhinhthuc").HasMaxLength(100).IsRequired();
        entity.Property(e => e.TrangThai).HasColumnName("trangthai").HasMaxLength(50).HasDefaultValue("HoatDong").IsRequired();
    }
}

public class DonHangConfiguration : IEntityTypeConfiguration<DonHang>
{
    public void Configure(EntityTypeBuilder<DonHang> entity)
    {
        entity.ToTable("don_hang");
        entity.HasKey(e => e.Id);
        entity.Property(e => e.Id).HasColumnName("id").ValueGeneratedOnAdd();
        entity.Property(e => e.CaLamViecId).HasColumnName("calamviecid");
        entity.Property(e => e.KhachHangId).HasColumnName("khachhangid");
        entity.Property(e => e.NguoiDungId).HasColumnName("nguoidungid").IsRequired();
        entity.Property(e => e.MaGiamGiaId).HasColumnName("magiamgiaid");
        entity.Property(e => e.HinhThucThanhToanId).HasColumnName("hinhthucthanhtoanid").IsRequired();
        entity.Property(e => e.NgayTao).HasColumnName("ngaytao").HasColumnType("timestamp without time zone").IsRequired();
        entity.Property(e => e.TongTienThanhToan).HasColumnName("tongtienthanhtoan").HasColumnType("numeric(18,2)").IsRequired();
        entity.Property(e => e.TrangThai).HasColumnName("trangthai").HasMaxLength(50).HasDefaultValue(TrangThaiDonHang.DangLapDon).IsRequired();

        entity.HasOne(e => e.NguoiDung)
            .WithMany()
            .HasForeignKey(e => e.NguoiDungId)
            .HasConstraintName("fk_donhang_nguoidung");

        entity.HasOne(e => e.MaGiamGia)
            .WithMany()
            .HasForeignKey(e => e.MaGiamGiaId)
            .HasConstraintName("fk_donhang_magiamgia");

        entity.HasOne(e => e.HinhThucThanhToan)
            .WithMany()
            .HasForeignKey(e => e.HinhThucThanhToanId)
            .HasConstraintName("fk_donhang_hinhthuctt");
    }
}

public class ChiTietDonHangConfiguration : IEntityTypeConfiguration<ChiTietDonHang>
{
    public void Configure(EntityTypeBuilder<ChiTietDonHang> entity)
    {
        entity.ToTable("chi_tiet_don_hang");
        entity.HasKey(e => new { e.DonHangId, e.SanPhamId });
        entity.Property(e => e.DonHangId).HasColumnName("donhangid");
        entity.Property(e => e.SanPhamId).HasColumnName("sanphamid");
        entity.Property(e => e.SoLuong).HasColumnName("soluong").IsRequired();
        entity.Property(e => e.DonGia).HasColumnName("dongia").HasColumnType("numeric(18,2)").IsRequired();
        entity.Property(e => e.ThanhTien).HasColumnName("thanhtien").HasColumnType("numeric(18,2)").IsRequired();

        entity.HasOne(e => e.DonHang)
            .WithMany(d => d.ChiTietDonHangs)
            .HasForeignKey(e => e.DonHangId)
            .HasConstraintName("fk_ctdh_donhang");

        entity.HasOne(e => e.SanPham)
            .WithMany()
            .HasForeignKey(e => e.SanPhamId)
            .HasConstraintName("fk_ctdh_sanpham");
    }
}
