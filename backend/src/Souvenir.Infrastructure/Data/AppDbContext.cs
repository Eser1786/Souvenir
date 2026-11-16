using Microsoft.EntityFrameworkCore;
using Souvenir.Application.Interfaces;
using Souvenir.Domain.Entities;

namespace Souvenir.Infrastructure.Data;

public class AppDbContext : DbContext, IApplicationDbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<NguoiDung> NguoiDungs => Set<NguoiDung>();
    public DbSet<VaiTro> VaiTros => Set<VaiTro>();
    public DbSet<ThamSo> ThamSos => Set<ThamSo>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<VaiTro>(entity =>
        {
            entity.ToTable("vai_tro");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id").ValueGeneratedOnAdd();
            entity.Property(e => e.TenVaiTro).HasColumnName("tenvaitro").HasMaxLength(50).IsRequired();
            entity.Property(e => e.MoTa).HasColumnName("mota").HasMaxLength(255);
        });

        modelBuilder.Entity<NguoiDung>(entity =>
        {
            entity.ToTable("nguoi_dung");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id").ValueGeneratedOnAdd();
            entity.Property(e => e.VaiTroId).HasColumnName("vaitroid").IsRequired();
            entity.Property(e => e.TenDangNhap).HasColumnName("tendangnhap").HasMaxLength(50).IsRequired();
            entity.Property(e => e.MatKhau).HasColumnName("matkhau").HasMaxLength(255).IsRequired();
            entity.Property(e => e.HoTen).HasColumnName("hoten").HasMaxLength(100).IsRequired();
            entity.Property(e => e.SoDienThoai).HasColumnName("sodienthoai").HasMaxLength(10).IsRequired();
            entity.Property(e => e.TrangThai).HasColumnName("trangthai").HasMaxLength(50).HasDefaultValue("HoatDong").IsRequired();
            entity.Property(e => e.SoLanDangNhapSai).HasColumnName("solandangnhapsai").HasDefaultValue(0).IsRequired();

            entity.HasOne(e => e.VaiTro)
                .WithMany(v => v.NguoiDungs)
                .HasForeignKey(e => e.VaiTroId)
                .HasConstraintName("fk_nguoidung_vaitro");
        });

        modelBuilder.Entity<ThamSo>(entity =>
        {
            entity.ToTable("tham_so");
            entity.HasKey(e => e.TenThamSo);
            entity.Property(e => e.TenThamSo).HasColumnName("tenthamso").HasMaxLength(100);
            entity.Property(e => e.KieuDuLieu).HasColumnName("kieudulieu").HasMaxLength(20).IsRequired();
            entity.Property(e => e.GiaTri).HasColumnName("giatri").HasMaxLength(255).IsRequired();
            entity.Property(e => e.MoTa).HasColumnName("mota").HasMaxLength(500);
        });
    }
}
