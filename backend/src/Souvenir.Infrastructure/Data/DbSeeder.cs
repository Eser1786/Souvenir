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

        var lockoutMinutesParam = await context.ThamSos.FirstOrDefaultAsync(t => t.TenThamSo == ThamSoConstants.ThoiGianKhoaTamThoiPhut);
        if (lockoutMinutesParam == null)
        {
            context.ThamSos.Add(new ThamSo
            {
                TenThamSo = ThamSoConstants.ThoiGianKhoaTamThoiPhut,
                KieuDuLieu = "int",
                GiaTri = "1",
                MoTa = "Thoi gian khoa tai khoan tam thoi sau khi vuot qua so lan dang nhap sai toi da (phut)"
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
            new { MaSP = "GB001", TenSP = "Gấu bông Teddy 25cm", DanhMuc = "Thú bông", DVT = "Cái", GiaNhap = 90000m, GiaBan = 180000m, Ton = 24, Image = "/products/teddy.png" },
            new { MaSP = "LY002", TenSP = "Ly sứ tặng Ba", DanhMuc = "Quà tặng", DVT = "Cái", GiaNhap = 45000m, GiaBan = 95000m, Ton = 36, Image = "/products/lysu.png" },
            new { MaSP = "DN003", TenSP = "Đèn ngủ gỗ", DanhMuc = "Đồ trang trí", DVT = "Cái", GiaNhap = 120000m, GiaBan = 220000m, Ton = 12, Image = "/products/denngu.png" },
            new { MaSP = "HQ004", TenSP = "Hộp quà pastel", DanhMuc = "Quà tặng", DVT = "Hộp", GiaNhap = 20000m, GiaBan = 45000m, Ton = 58, Image = "/products/hopqua.png" },
            new { MaSP = "BT005", TenSP = "Bộ bút quà tặng", DanhMuc = "Văn phòng phẩm", DVT = "Bộ", GiaNhap = 60000m, GiaBan = 120000m, Ton = 18, Image = "/products/but.png" },
            new { MaSP = "DL006", TenSP = "Đèn lồng trang trí", DanhMuc = "Đồ trang trí", DVT = "Cái", GiaNhap = 130000m, GiaBan = 250000m, Ton = 9, Image = "/products/denlong.png" },

            // Danh mục sản phẩm mẫu lưu niệm bổ sung
            new { MaSP = "SP001", TenSP = "Nam châm tủ lạnh Việt Nam", DanhMuc = "Đồ lưu niệm", DVT = "Cái", GiaNhap = 12000m, GiaBan = 25000m, Ton = 50, Image = "/products/namcham.png" },
            new { MaSP = "SP002", TenSP = "Mô hình xe xích lô mini", DanhMuc = "Đồ lưu niệm", DVT = "Cái", GiaNhap = 45000m, GiaBan = 85000m, Ton = 30, Image = "/products/xichlo.png" },
            new { MaSP = "SP003", TenSP = "Móc khóa nón lá", DanhMuc = "Móc khóa", DVT = "Cái", GiaNhap = 8000m, GiaBan = 20000m, Ton = 80, Image = "/products/mockhoa.png" },
            new { MaSP = "SP004", TenSP = "Móc khóa landmark", DanhMuc = "Móc khóa", DVT = "Cái", GiaNhap = 10000m, GiaBan = 25000m, Ton = 65, Image = "/products/mockhoa_landmark.png" },
            new { MaSP = "SP005", TenSP = "Quạt giấy thủ công", DanhMuc = "Đồ thủ công mỹ nghệ", DVT = "Cái", GiaNhap = 25000m, GiaBan = 50000m, Ton = 40, Image = "/products/quatgiay.png" },
            new { MaSP = "SP006", TenSP = "Bộ vòng tay handmade", DanhMuc = "Trang sức và phụ kiện", DVT = "Bộ", GiaNhap = 30000m, GiaBan = 65000m, Ton = 35, Image = "/products/vongtay.png" },
            new { MaSP = "SP007", TenSP = "Sổ tay họa tiết Việt Nam", DanhMuc = "Văn phòng phẩm", DVT = "Quyển", GiaNhap = 18000m, GiaBan = 40000m, Ton = 55, Image = "/products/sotay.jpg" },
            new { MaSP = "SP008", TenSP = "Tranh trang trí mini", DanhMuc = "Đồ trang trí", DVT = "Cái", GiaNhap = 35000m, GiaBan = 75000m, Ton = 25, Image = "/products/denngu.png" },
            new { MaSP = "SP009", TenSP = "Túi vải in họa tiết", DanhMuc = "Áo thun và túi vải", DVT = "Cái", GiaNhap = 28000m, GiaBan = 60000m, Ton = 45, Image = "/products/tuivai.jpg" },
        };

        foreach (var p in sampleProducts)
        {
            var dmList = await context.DanhMucSanPhams.ToListAsync();
            var dm = dmList.FirstOrDefault(d => 
                Souvenir.Application.Common.StringNormalizer.EqualsNormalized(d.TenDanhMuc, p.DanhMuc));
            if (dm == null)
            {
                dm = new DanhMucSanPham { TenDanhMuc = p.DanhMuc, TrangThai = "HoatDong" };
                context.DanhMucSanPhams.Add(dm);
                await context.SaveChangesAsync();
            }
            else if (dm.TenDanhMuc != p.DanhMuc)
            {
                // Cập nhật lại tên danh mục có dấu chuẩn
                dm.TenDanhMuc = p.DanhMuc;
                await context.SaveChangesAsync();
            }

            var dvtList = await context.DonViTinhs.ToListAsync();
            var dvt = dvtList.FirstOrDefault(d => 
                Souvenir.Application.Common.StringNormalizer.EqualsNormalized(d.TenDonVi, p.DVT));
            if (dvt == null)
            {
                dvt = new DonViTinh { TenDonVi = p.DVT };
                context.DonViTinhs.Add(dvt);
                await context.SaveChangesAsync();
            }
            else if (dvt.TenDonVi != p.DVT)
            {
                // Cập nhật lại tên đơn vị tính có dấu chuẩn
                dvt.TenDonVi = p.DVT;
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

        // 6. Seed Lịch sử Ca & Đơn hàng mẫu nếu chưa có dữ liệu đơn hàng (để biểu đồ báo cáo UC14 có dữ liệu ban đầu)
        if (!await context.DonHangs.AnyAsync())
        {
            var staff = await context.NguoiDungs.FirstOrDefaultAsync(u => u.TenDangNhap == "tranthuha")
                        ?? await context.NguoiDungs.FirstAsync();
            var customer1 = await context.KhachHangs.FirstOrDefaultAsync(c => c.SoDienThoai == "0901234567")
                            ?? await context.KhachHangs.FirstAsync();
            var customer2 = await context.KhachHangs.FirstOrDefaultAsync(c => c.SoDienThoai == "0987654321");

            var spTeddy = await context.SanPhams.FirstOrDefaultAsync(s => s.MaSP == "GB001");
            var spLySu = await context.SanPhams.FirstOrDefaultAsync(s => s.MaSP == "LY002");
            var spDenNgu = await context.SanPhams.FirstOrDefaultAsync(s => s.MaSP == "DN003");
            var spHopQua = await context.SanPhams.FirstOrDefaultAsync(s => s.MaSP == "HQ004");
            var voucher10 = await context.MaGiamGias.FirstOrDefaultAsync(v => v.MaCode == "QUATANG10");

            // Tạo ca làm việc hôm qua đã chốt
            var pastShift = new CaLamViec
            {
                NguoiDungId = staff.Id,
                ThoiGianMoCa = DateTime.UtcNow.AddDays(-1).Date.AddHours(8),
                ThoiGianChotCa = DateTime.UtcNow.AddDays(-1).Date.AddHours(17),
                TienMatBanGiao = 1000000m,
                TienMatThucTe = 1530000m,
                TrangThai = "DA_CHOT"
            };
            context.CaLamViecs.Add(pastShift);
            await context.SaveChangesAsync();

            // Đơn hàng 1 (Hôm qua - Tiền mặt)
            if (spTeddy != null && spLySu != null)
            {
                var don1 = new DonHang
                {
                    CaLamViecId = pastShift.Id,
                    KhachHangId = customer1.Id,
                    NguoiDungId = staff.Id,
                    HinhThucThanhToanId = 1, // Tiền mặt
                    NgayTao = DateTime.UtcNow.AddDays(-1).Date.AddHours(10).AddMinutes(30),
                    TrangThai = TrangThaiDonHang.DaThanhToan,
                    MaGiamGiaId = voucher10?.Id,
                    TongTienThanhToan = 247500m // (180k + 95k) * 0.9 = 247.5k
                };
                don1.ChiTietDonHangs.Add(new ChiTietDonHang
                {
                    SanPhamId = spTeddy.Id,
                    SoLuong = 1,
                    DonGia = spTeddy.GiaBan,
                    ThanhTien = spTeddy.GiaBan
                });
                don1.ChiTietDonHangs.Add(new ChiTietDonHang
                {
                    SanPhamId = spLySu.Id,
                    SoLuong = 1,
                    DonGia = spLySu.GiaBan,
                    ThanhTien = spLySu.GiaBan
                });
                context.DonHangs.Add(don1);
            }

            // Đơn hàng 2 (Hôm qua - Chuyển khoản)
            if (spDenNgu != null && spHopQua != null)
            {
                var don2 = new DonHang
                {
                    CaLamViecId = pastShift.Id,
                    KhachHangId = customer2?.Id,
                    NguoiDungId = staff.Id,
                    HinhThucThanhToanId = 2, // Chuyển khoản / QR
                    NgayTao = DateTime.UtcNow.AddDays(-1).Date.AddHours(15).AddMinutes(15),
                    TrangThai = TrangThaiDonHang.DaThanhToan,
                    TongTienThanhToan = 265000m // 220k + 45k
                };
                don2.ChiTietDonHangs.Add(new ChiTietDonHang
                {
                    SanPhamId = spDenNgu.Id,
                    SoLuong = 1,
                    DonGia = spDenNgu.GiaBan,
                    ThanhTien = spDenNgu.GiaBan
                });
                don2.ChiTietDonHangs.Add(new ChiTietDonHang
                {
                    SanPhamId = spHopQua.Id,
                    SoLuong = 1,
                    DonGia = spHopQua.GiaBan,
                    ThanhTien = spHopQua.GiaBan
                });
                context.DonHangs.Add(don2);
            }

            // Đơn hàng 3 (2 ngày trước - Tiền mặt)
            if (spTeddy != null)
            {
                var don3 = new DonHang
                {
                    CaLamViecId = pastShift.Id,
                    KhachHangId = customer1.Id,
                    NguoiDungId = staff.Id,
                    HinhThucThanhToanId = 1,
                    NgayTao = DateTime.UtcNow.AddDays(-2).Date.AddHours(14).AddMinutes(20),
                    TrangThai = TrangThaiDonHang.DaThanhToan,
                    TongTienThanhToan = 360000m // 2 * 180k
                };
                don3.ChiTietDonHangs.Add(new ChiTietDonHang
                {
                    SanPhamId = spTeddy.Id,
                    SoLuong = 2,
                    DonGia = spTeddy.GiaBan,
                    ThanhTien = spTeddy.GiaBan * 2
                });
                context.DonHangs.Add(don3);
            }
            await context.SaveChangesAsync();
        }

        // 8. Seed Nhà cung cấp mặc định nếu chưa có
        if (!await context.NhaCungCaps.AnyAsync())
        {
            context.NhaCungCaps.AddRange(
                new NhaCungCap { MaNCC = "NCC001", TenNCC = "Cơ sở quà tặng Việt An", SoDienThoai = "0900000001", DiaChi = "Quận 5, TP. Hồ Chí Minh" },
                new NhaCungCap { MaNCC = "NCC002", TenNCC = "Cơ sở thủ công Mỹ Nghệ Thành Đạt", SoDienThoai = "0900000002", DiaChi = "Quận 6, TP. Hồ Chí Minh" },
                new NhaCungCap { MaNCC = "NCC003", TenNCC = "Nhà phân phối Văn Phòng Phẩm Minh Châu", SoDienThoai = "0900000003", DiaChi = "Quận 10, TP. Hồ Chí Minh" },
                new NhaCungCap { MaNCC = "NCC004", TenNCC = "Xưởng may Quà Tặng Sao Việt", SoDienThoai = "0900000004", DiaChi = "Quận Tân Bình, TP. Hồ Chí Minh" }
            );
            await context.SaveChangesAsync();
        }
    }
}
