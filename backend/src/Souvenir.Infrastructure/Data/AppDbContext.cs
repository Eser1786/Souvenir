using Microsoft.EntityFrameworkCore;
using Souvenir.Application.Interfaces;
using Souvenir.Domain.Entities;
using Souvenir.Infrastructure.Data.Configurations;

namespace Souvenir.Infrastructure.Data;

public class AppDbContext : DbContext, IApplicationDbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<NguoiDung> NguoiDungs => Set<NguoiDung>();
    public DbSet<VaiTro> VaiTros => Set<VaiTro>();
    public DbSet<ThamSo> ThamSos => Set<ThamSo>();
    public DbSet<SanPham> SanPhams => Set<SanPham>();
    public DbSet<DanhMucSanPham> DanhMucSanPhams => Set<DanhMucSanPham>();
    public DbSet<DonViTinh> DonViTinhs => Set<DonViTinh>();
    public DbSet<MaGiamGia> MaGiamGias => Set<MaGiamGia>();
    public DbSet<HinhThucThanhToan> HinhThucThanhToans => Set<HinhThucThanhToan>();
    public DbSet<DonHang> DonHangs => Set<DonHang>();
    public DbSet<ChiTietDonHang> ChiTietDonHangs => Set<ChiTietDonHang>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // UC07 - Đơn hàng & Thanh toán (cấu hình tách riêng trong Data/Configurations)
        modelBuilder.ApplyConfiguration(new HinhThucThanhToanConfiguration());
        modelBuilder.ApplyConfiguration(new DonHangConfiguration());
        modelBuilder.ApplyConfiguration(new ChiTietDonHangConfiguration());

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

        modelBuilder.Entity<DanhMucSanPham>(entity =>
        {
            entity.ToTable("danh_muc_san_pham");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id").ValueGeneratedOnAdd();
            entity.Property(e => e.TenDanhMuc).HasColumnName("tendanhmuc").HasMaxLength(100).IsRequired();
            entity.Property(e => e.TrangThai).HasColumnName("trangthai").HasMaxLength(50).HasDefaultValue("HoatDong").IsRequired();
        });

        modelBuilder.Entity<DonViTinh>(entity =>
        {
            entity.ToTable("don_vi_tinh");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id").ValueGeneratedOnAdd();
            entity.Property(e => e.TenDonVi).HasColumnName("tendonvi").HasMaxLength(50).IsRequired();
        });

        modelBuilder.Entity<SanPham>(entity =>
        {
            entity.ToTable("san_pham");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id").ValueGeneratedOnAdd();
            entity.Property(e => e.DanhMucId).HasColumnName("danhmucid").IsRequired();
            entity.Property(e => e.DonViTinhId).HasColumnName("donvitinhid").IsRequired();
            entity.Property(e => e.MaSP).HasColumnName("masp").HasMaxLength(50).IsRequired();
            entity.Property(e => e.HinhAnh).HasColumnName("imageurl").HasMaxLength(255);
            entity.Property(e => e.TenSP).HasColumnName("tensp").HasMaxLength(150).IsRequired();
            entity.Property(e => e.GiaNhap).HasColumnName("gianhap").HasColumnType("numeric(18,2)").IsRequired();
            entity.Property(e => e.GiaBan).HasColumnName("giaban").HasColumnType("numeric(18,2)").IsRequired();
            entity.Property(e => e.SoLuongTon).HasColumnName("soluongton").HasDefaultValue(0).IsRequired();
            entity.Property(e => e.TrangThai).HasColumnName("trangthai").HasMaxLength(50).HasDefaultValue("KinhDoanh").IsRequired();

            entity.HasOne(e => e.DanhMuc)
                .WithMany(d => d.SanPhams)
                .HasForeignKey(e => e.DanhMucId)
                .HasConstraintName("fk_sanpham_danhmuc");

            entity.HasOne(e => e.DonViTinh)
                .WithMany(d => d.SanPhams)
                .HasForeignKey(e => e.DonViTinhId)
                .HasConstraintName("fk_sanpham_donvitinh");
        });

        modelBuilder.Entity<MaGiamGia>(entity =>
        {
            entity.ToTable("ma_giam_gia");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id").ValueGeneratedOnAdd();
            entity.Property(e => e.MaCode).HasColumnName("macode").HasMaxLength(50).IsRequired();
            entity.Property(e => e.TenVoucher).HasColumnName("tenvoucher").HasMaxLength(150).IsRequired();
            entity.Property(e => e.LoaiGiamGia).HasColumnName("loaigiamgia").HasMaxLength(50).IsRequired();
            entity.Property(e => e.GiaTriGiam).HasColumnName("giatrigiam").HasColumnType("numeric(18,2)").IsRequired();
            entity.Property(e => e.NgayBatDau).HasColumnName("ngaybatdau").IsRequired();
            entity.Property(e => e.NgayKetThuc).HasColumnName("ngayketthuc").IsRequired();
        });
    }
}
