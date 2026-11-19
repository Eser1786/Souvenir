using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Souvenir.Domain.Entities;

namespace Souvenir.Infrastructure.Data.Configurations;

public class NhaCungCapConfiguration : IEntityTypeConfiguration<NhaCungCap>
{
    public void Configure(EntityTypeBuilder<NhaCungCap> entity)
    {
        entity.ToTable("nha_cung_cap");
        entity.HasKey(e => e.Id);
        entity.Property(e => e.Id).HasColumnName("id").ValueGeneratedOnAdd();
        entity.Property(e => e.MaNCC).HasColumnName("mancc").HasMaxLength(50).IsRequired();
        entity.HasIndex(e => e.MaNCC).IsUnique();
        entity.Property(e => e.TenNCC).HasColumnName("tenncc").HasMaxLength(150).IsRequired();
        entity.Property(e => e.SoDienThoai).HasColumnName("sodienthoai").HasMaxLength(15).IsRequired();
        entity.Property(e => e.DiaChi).HasColumnName("diachi").HasMaxLength(255);
    }
}

public class PhieuNhapKhoConfiguration : IEntityTypeConfiguration<PhieuNhapKho>
{
    public void Configure(EntityTypeBuilder<PhieuNhapKho> entity)
    {
        entity.ToTable("phieu_nhap_kho");
        entity.HasKey(e => e.Id);
        entity.Property(e => e.Id).HasColumnName("id").ValueGeneratedOnAdd();
        entity.Property(e => e.MaPhieu).HasColumnName("maphieu").HasMaxLength(50).IsRequired();
        entity.HasIndex(e => e.MaPhieu).IsUnique();
        entity.Property(e => e.NhaCungCapId).HasColumnName("nhacungcapid").IsRequired();
        entity.Property(e => e.NguoiDungId).HasColumnName("nguoidungid").IsRequired();
        entity.Property(e => e.TongTienNhap).HasColumnName("tongtiennhap").HasColumnType("numeric(18,2)").IsRequired();
        entity.Property(e => e.NgayNhap).HasColumnName("ngaynhap").HasColumnType("timestamp without time zone").IsRequired();

        entity.HasOne(e => e.NhaCungCap)
            .WithMany(n => n.PhieuNhapKhos)
            .HasForeignKey(e => e.NhaCungCapId)
            .HasConstraintName("fk_pnk_nhacungcap");

        entity.HasOne(e => e.NguoiDung)
            .WithMany()
            .HasForeignKey(e => e.NguoiDungId)
            .HasConstraintName("fk_pnk_nguoidung");
    }
}

public class ChiTietNhapKhoConfiguration : IEntityTypeConfiguration<ChiTietNhapKho>
{
    public void Configure(EntityTypeBuilder<ChiTietNhapKho> entity)
    {
        entity.ToTable("chi_tiet_nhap_kho");
        entity.HasKey(e => new { e.PhieuNhapKhoId, e.SanPhamId });
        entity.Property(e => e.PhieuNhapKhoId).HasColumnName("phieunhapkhoid");
        entity.Property(e => e.SanPhamId).HasColumnName("sanphamid");
        entity.Property(e => e.SoLuongNhap).HasColumnName("soluongnhap").IsRequired();
        entity.Property(e => e.DonGiaNhap).HasColumnName("dongianhap").HasColumnType("numeric(18,2)").IsRequired();
        entity.Property(e => e.ThanhTien).HasColumnName("thanhtien").HasColumnType("numeric(18,2)").IsRequired();

        entity.HasOne(e => e.PhieuNhapKho)
            .WithMany(p => p.ChiTietNhapKhos)
            .HasForeignKey(e => e.PhieuNhapKhoId)
            .HasConstraintName("fk_ctnk_phieunhapkho");

        entity.HasOne(e => e.SanPham)
            .WithMany()
            .HasForeignKey(e => e.SanPhamId)
            .HasConstraintName("fk_ctnk_sanpham");
    }
}
