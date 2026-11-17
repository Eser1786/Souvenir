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

        // 4. Seed / Update Sample Products with stock & high-quality images
        var sampleProducts = new[]
        {
            // 6 sản phẩm chuẩn Figma Screen 02 (Ưu tiên hiển thị đầu tiên)
            new { MaSP = "GB001", TenSP = "Gấu bông Teddy 25cm", DanhMuc = "Thu bong", DVT = "Cai", GiaNhap = 90000m, GiaBan = 180000m, Ton = 24, Image = "/products/teddy.png" },
            new { MaSP = "LY002", TenSP = "Ly sứ tặng Ba", DanhMuc = "Qua tang", DVT = "Cai", GiaNhap = 45000m, GiaBan = 95000m, Ton = 36, Image = "/products/lysu.png" },
            new { MaSP = "DN003", TenSP = "Đèn ngủ gỗ", DanhMuc = "Do trang tri", DVT = "Cai", GiaNhap = 120000m, GiaBan = 220000m, Ton = 12, Image = "/products/denngu.png" },
            new { MaSP = "HQ004", TenSP = "Hộp quà pastel", DanhMuc = "Qua tang", DVT = "Hop", GiaNhap = 20000m, GiaBan = 45000m, Ton = 58, Image = "/products/hopqua.png" },
            new { MaSP = "BT005", TenSP = "Bộ bút quà tặng", DanhMuc = "Van phong pham", DVT = "Bo", GiaNhap = 60000m, GiaBan = 120000m, Ton = 18, Image = "/products/but.png" },
            new { MaSP = "DL006", TenSP = "Đèn lồng trang trí", DanhMuc = "Do trang tri", DVT = "Cai", GiaNhap = 130000m, GiaBan = 250000m, Ton = 9, Image = "/products/denlong.png" },

            // Danh mục sản phẩm mẫu lưu niệm bổ sung
            new { MaSP = "SP001", TenSP = "Nam châm tủ lạnh Việt Nam", DanhMuc = "Do luu niem", DVT = "Cai", GiaNhap = 12000m, GiaBan = 25000m, Ton = 50, Image = "/products/namcham.jpg" },
            new { MaSP = "SP002", TenSP = "Mô hình xe xích lô mini", DanhMuc = "Do luu niem", DVT = "Cai", GiaNhap = 45000m, GiaBan = 85000m, Ton = 30, Image = "/products/xichlo.jpg" },
            new { MaSP = "SP003", TenSP = "Móc khóa nón lá", DanhMuc = "Moc khoa", DVT = "Cai", GiaNhap = 8000m, GiaBan = 20000m, Ton = 80, Image = "/products/mockhoa.jpg" },
            new { MaSP = "SP004", TenSP = "Móc khóa landmark", DanhMuc = "Moc khoa", DVT = "Cai", GiaNhap = 10000m, GiaBan = 25000m, Ton = 65, Image = "/products/mockhoa_landmark.jpg" },
            new { MaSP = "SP005", TenSP = "Quạt giấy thủ công", DanhMuc = "Do thu cong my nghe", DVT = "Cai", GiaNhap = 25000m, GiaBan = 50000m, Ton = 40, Image = "/products/quatgiay.jpg" },
            new { MaSP = "SP006", TenSP = "Bộ vòng tay handmade", DanhMuc = "Trang suc va phu kien", DVT = "Bo", GiaNhap = 30000m, GiaBan = 65000m, Ton = 35, Image = "/products/vongtay.jpg" },
            new { MaSP = "SP007", TenSP = "Sổ tay họa tiết Việt Nam", DanhMuc = "Van phong pham", DVT = "Quyen", GiaNhap = 18000m, GiaBan = 40000m, Ton = 55, Image = "/products/sotay.jpg" },
            new { MaSP = "SP008", TenSP = "Tranh trang trí mini", DanhMuc = "Do trang tri", DVT = "Cai", GiaNhap = 35000m, GiaBan = 75000m, Ton = 25, Image = "/products/denngu.png" },
            new { MaSP = "SP009", TenSP = "Túi vải in họa tiết", DanhMuc = "Ao thun va tui vai", DVT = "Cai", GiaNhap = 28000m, GiaBan = 60000m, Ton = 45, Image = "/products/tuivai.jpg" },
            new { MaSP = "SP010", TenSP = "Hộp quà lưu niệm", DanhMuc = "Qua tang", DVT = "Hop", GiaNhap = 40000m, GiaBan = 90000m, Ton = 30, Image = "/products/hopqua.png" },
        };

        foreach (var p in sampleProducts)
        {
            var dm = await context.DanhMucSanPhams.FirstOrDefaultAsync(d => d.TenDanhMuc == p.DanhMuc);
            if (dm == null)
            {
                dm = new DanhMucSanPham { TenDanhMuc = p.DanhMuc, TrangThai = "HoatDong" };
                context.DanhMucSanPhams.Add(dm);
                await context.SaveChangesAsync();
            }

            var dvt = await context.DonViTinhs.FirstOrDefaultAsync(d => d.TenDonVi == p.DVT);
            if (dvt == null)
            {
                dvt = new DonViTinh { TenDonVi = p.DVT };
                context.DonViTinhs.Add(dvt);
                await context.SaveChangesAsync();
            }

            var existingSp = await context.SanPhams.FirstOrDefaultAsync(s => s.MaSP == p.MaSP);
            if (existingSp != null)
            {
                if (existingSp.SoLuongTon <= 0)
                {
                    existingSp.SoLuongTon = p.Ton;
                }
                existingSp.HinhAnh = p.Image;
                existingSp.TenSP = p.TenSP;
                existingSp.GiaNhap = p.GiaNhap;
                existingSp.GiaBan = p.GiaBan;
                existingSp.DanhMucId = dm.Id;
                existingSp.DonViTinhId = dvt.Id;
            }
            else
            {
                context.SanPhams.Add(new SanPham
                {
                    MaSP = p.MaSP,
                    TenSP = p.TenSP,
                    DanhMucId = dm.Id,
                    DonViTinhId = dvt.Id,
                    GiaNhap = p.GiaNhap,
                    GiaBan = p.GiaBan,
                    SoLuongTon = p.Ton,
                    HinhAnh = p.Image,
                    TrangThai = "KinhDoanh"
                });
            }
        }

        // 5. Seed Default Customers if none exist
        if (!await context.KhachHangs.AnyAsync())
        {
            context.KhachHangs.AddRange(
                new KhachHang { TenKhachHang = "Nguyễn Ngọc Lan", SoDienThoai = "0901234567", DiemTichLuy = 250 },
                new KhachHang { TenKhachHang = "Trần Văn An", SoDienThoai = "0987654321", DiemTichLuy = 120 },
                new KhachHang { TenKhachHang = "Lê Thị Mai", SoDienThoai = "0912345678", DiemTichLuy = 50 },
                new KhachHang { TenKhachHang = "Phạm Hoàng Nam", SoDienThoai = "0933456789", DiemTichLuy = 520 },
                new KhachHang { TenKhachHang = "Đỗ Phương Thảo", SoDienThoai = "0977889900", DiemTichLuy = 15 }
            );
            await context.SaveChangesAsync();
        }

        await context.SaveChangesAsync();

        // 5. Seed Mã giảm giá (Vouchers)
        var sampleVouchers = new[]
        {
            new { MaCode = "QUATANG10", Ten = "Giảm giá 10% Quà Tặng", Loai = "PhanTram", GiaTri = 10m, DaysValid = 90 },
            new { MaCode = "GIAM10K", Ten = "Giảm 10.000 đ đơn hàng", Loai = "SoTien", GiaTri = 10000m, DaysValid = 90 },
            new { MaCode = "TRIAN50K", Ten = "Tri ân khách hàng giảm 50.000 đ", Loai = "SoTien", GiaTri = 50000m, DaysValid = 90 },
            new { MaCode = "HETHAN", Ten = "Mã ưu đãi đã hết hạn", Loai = "PhanTram", GiaTri = 20m, DaysValid = -5 },
        };

        foreach (var v in sampleVouchers)
        {
            var existingVoucher = await context.MaGiamGias.FirstOrDefaultAsync(m => m.MaCode == v.MaCode);
            if (existingVoucher == null)
            {
                context.MaGiamGias.Add(new MaGiamGia
                {
                    MaCode = v.MaCode,
                    TenVoucher = v.Ten,
                    LoaiGiamGia = v.Loai,
                    GiaTriGiam = v.GiaTri,
                    NgayBatDau = DateTime.UtcNow.AddDays(-30),
                    NgayKetThuc = DateTime.UtcNow.AddDays(v.DaysValid)
                });
            }
        }

        await context.SaveChangesAsync();
    }
}
