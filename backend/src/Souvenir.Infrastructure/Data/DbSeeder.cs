using Microsoft.EntityFrameworkCore;
using Souvenir.Application.Interfaces;
using Souvenir.Domain.Constants;
using Souvenir.Domain.Entities;

namespace Souvenir.Infrastructure.Data;

public static class DbSeeder
{
    public static async Task SeedAsync(AppDbContext context, IPasswordHasher passwordHasher)
    {
        // 1. Seed Roles
        var quanLyRole = await context.VaiTros.FirstOrDefaultAsync(r => r.TenVaiTro == VaiTroConstants.QuanLy);
        if (quanLyRole == null)
        {
            quanLyRole = new VaiTro
            {
                TenVaiTro = VaiTroConstants.QuanLy,
                MoTa = "Quan ly danh muc, nhan su, kho hang, ban hang va bao cao"
            };
            context.VaiTros.Add(quanLyRole);
            await context.SaveChangesAsync();
        }

        var nhanVienRole = await context.VaiTros.FirstOrDefaultAsync(r => r.TenVaiTro == VaiTroConstants.NhanVien);
        if (nhanVienRole == null)
        {
            nhanVienRole = new VaiTro
            {
                TenVaiTro = VaiTroConstants.NhanVien,
                MoTa = "Nhan vien thuc hien cac cong viec ban hang va ho tro cua hang"
            };
            context.VaiTros.Add(nhanVienRole);
            await context.SaveChangesAsync();
        }

        // 2. Seed Default Parameters
        var maxAttemptsParam = await context.ThamSos.FirstOrDefaultAsync(t => t.TenThamSo == ThamSoConstants.SoLanDangNhapSaiToiDa);
        if (maxAttemptsParam == null)
        {
            context.ThamSos.Add(new ThamSo
            {
                TenThamSo = ThamSoConstants.SoLanDangNhapSaiToiDa,
                KieuDuLieu = "int",
                GiaTri = "5",
                MoTa = "So lan dang nhap sai toi da truoc khi ap dung co che khoa tam thoi"
            });
            await context.SaveChangesAsync();
        }

        // 3. Seed Default Users
        var adminUser = await context.NguoiDungs.FirstOrDefaultAsync(u => u.TenDangNhap == "admin");
        if (adminUser == null)
        {
            adminUser = new NguoiDung
            {
                VaiTroId = quanLyRole.Id,
                TenDangNhap = "admin",
                MatKhau = passwordHasher.HashPassword("admin123"),
                HoTen = "Quản Lý Cửa Hàng",
                SoDienThoai = "0909888999",
                TrangThai = TrangThaiTaiKhoan.HoatDong,
                SoLanDangNhapSai = 0
            };
            context.NguoiDungs.Add(adminUser);
        }

        var staffUser = await context.NguoiDungs.FirstOrDefaultAsync(u => u.TenDangNhap == "tranthuha");
        if (staffUser == null)
        {
            staffUser = new NguoiDung
            {
                VaiTroId = nhanVienRole.Id,
                TenDangNhap = "tranthuha",
                MatKhau = passwordHasher.HashPassword("password123"),
                HoTen = "Trần Thu Hà",
                SoDienThoai = "0901234567",
                TrangThai = TrangThaiTaiKhoan.HoatDong,
                SoLanDangNhapSai = 0
            };
            context.NguoiDungs.Add(staffUser);
        }

        await context.SaveChangesAsync();
    }
}
