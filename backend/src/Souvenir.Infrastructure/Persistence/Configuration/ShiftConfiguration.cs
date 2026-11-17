using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Souvenir.Domain.Entities;

namespace Souvenir.Infrastructure.Persistence.Configurations;

public class CaLamViecConfiguration : IEntityTypeConfiguration<CaLamViec>
{
    public void Configure(EntityTypeBuilder<CaLamViec> builder)
    {
        builder.ToTable("ca_lam_viec");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnName("id").UseIdentityAlwaysColumn();
        builder.Property(x => x.NguoiDungId).HasColumnName("nguoidungid");
        builder.Property(x => x.ThoiGianMoCa).HasColumnName("thoigianmoca");
        builder.Property(x => x.ThoiGianChotCa).HasColumnName("thoigianchotca");
        builder.Property(x => x.TienMatBanGiao).HasColumnName("tienmatbangiao");
        builder.Property(x => x.TienMatThucTe).HasColumnName("tienmatthucte");
        builder.Property(x => x.TrangThai).HasColumnName("trangthai");
        builder.Property(x => x.LyDoChenhLech).HasColumnName("lydochenhlech");
    }
}

public class ChamCongConfiguration : IEntityTypeConfiguration<ChamCong>
{
    public void Configure(EntityTypeBuilder<ChamCong> builder)
    {
        builder.ToTable("cham_cong");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnName("id").UseIdentityAlwaysColumn();
        builder.Property(x => x.NguoiDungId).HasColumnName("nguoidungid");
        builder.Property(x => x.CaLamViecId).HasColumnName("calamviecid");
        builder.Property(x => x.GioCheckIn).HasColumnName("giocheckin");
        builder.Property(x => x.GioCheckOut).HasColumnName("giocheckout");

        builder.HasOne(x => x.CaLamViec)
               .WithOne(c => c.ChamCong)
               .HasForeignKey<ChamCong>(x => x.CaLamViecId);
    }
}