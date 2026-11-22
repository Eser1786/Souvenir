-- ============================================================
-- SEED DATA - QUAN LY CUA HANG LUU NIEM
-- Du lieu khoi tao / danh muc tham chieu, khong tao giao dich.
--
-- Cach dung:
--   1. Chay schema.sql de tao cac bang truoc.
--   2. Chay file nay de nap du lieu mau.
--
-- File co the chay lai: cac lenh INSERT su dung ON CONFLICT
-- DO NOTHING de tranh chen trung du lieu co khoa/unique trung.
--
-- Khong tao: don hang, chi tiet don hang, phieu nhap, phieu tra,
-- phieu kiem ke, ca lam viec, cham cong.
-- Khong tao tai khoan nguoi dung vi mat khau can duoc hash
-- bang BCrypt tu ung dung.
-- ============================================================


-- ============================================================
-- 1. THAM SO HE THONG
-- ============================================================

INSERT INTO tham_so (tenThamSo, kieuDuLieu, giaTri, moTa)
VALUES
    ('SoLanDangNhapSaiToiDa', 'int', '5',
     'So lan dang nhap sai toi da truoc khi ap dung co che khoa tam thoi'),
    ('ThoiGianKhoaTamThoiPhut', 'int', '1',
     'Thoi gian khoa tai khoan tam thoi tinh theo phut'),
    ('SoNgayChoPhepDoiTra', 'int', '7',
     'So ngay toi da cho phep doi tra tinh tu ngay mua'),
    ('TyLeTichDiem', 'decimal', '0.01',
     'Ty le tich diem tren gia tri hoa don, tuong duong 1 phan tram'),
    ('TyLeDoiDiem', 'decimal', '1000',
     'So tien giam tuong ung voi mot diem tich luy, don vi VND'),
    ('DiemToiThieuDoiThuong', 'int', '10',
     'So diem tich luy toi thieu de bat dau doi thuong'),
    ('SoLuongCanhBaoTonKho', 'int', '5',
     'Canh bao khi so luong ton kho bang hoac thap hon nguong nay')
ON CONFLICT (tenThamSo) DO NOTHING;


-- ============================================================
-- 2. VAI TRO
-- ============================================================

INSERT INTO vai_tro (tenVaiTro, moTa)
VALUES
    ('QuanLy', 'Quan ly danh muc, nhan su, kho hang, ban hang va bao cao'),
    ('NhanVien', 'Nhan vien thuc hien cac cong viec ban hang va ho tro cua hang')
ON CONFLICT (tenVaiTro) DO NOTHING;


-- ============================================================
-- 3. DANH MUC SAN PHAM
-- ============================================================

INSERT INTO danh_muc_san_pham (tenDanhMuc, trangThai)
VALUES
    ('Do luu niem', 'HoatDong'),
    ('Moc khoa', 'HoatDong'),
    ('Do thu cong my nghe', 'HoatDong'),
    ('Trang suc va phu kien', 'HoatDong'),
    ('Van phong pham', 'HoatDong'),
    ('Do trang tri', 'HoatDong'),
    ('Ao thun va tui vai', 'HoatDong'),
    ('Qua tang', 'HoatDong')
ON CONFLICT (tenDanhMuc) DO NOTHING;


-- ============================================================
-- 4. DON VI TINH
-- ============================================================

INSERT INTO don_vi_tinh (tenDonVi)
VALUES
    ('Cai'),
    ('Hop'),
    ('Bo'),
    ('Con'),
    ('Chiec'),
    ('Quyen'),
    ('Tui'),
    ('Tam')
ON CONFLICT (tenDonVi) DO NOTHING;


-- ============================================================
-- 5. HINH THUC THANH TOAN
-- ============================================================

INSERT INTO hinh_thuc_thanh_toan (tenHinhThuc, trangThai)
VALUES
    ('Tien mat', 'HoatDong'),
    ('Chuyen khoan QR', 'HoatDong'),
    ('The ngan hang', 'HoatDong'),
    ('Vi dien tu', 'HoatDong')
ON CONFLICT (tenHinhThuc) DO NOTHING;


-- ============================================================
-- 6. NHA CUNG CAP MAU
-- Day la du lieu danh muc doi tac, khong phai phieu nhap kho.
-- Ten va thong tin duoi day deu la du lieu gia lap de demo.
-- ============================================================

INSERT INTO nha_cung_cap (maNCC, tenNCC, soDienThoai, diaChi)
VALUES
    ('NCC001', 'Co so qua tang Viet An', '0900000001', 'Quan 5, TP. Ho Chi Minh'),
    ('NCC002', 'Co so thu cong My Nghe Thanh Dat', '0900000002', 'Quan 6, TP. Ho Chi Minh'),
    ('NCC003', 'Nha phan phoi Van Phong Pham Minh Chau', '0900000003', 'Quan 10, TP. Ho Chi Minh'),
    ('NCC004', 'Xuong may Qua Tang Sao Viet', '0900000004', 'Quan Tan Binh, TP. Ho Chi Minh')
ON CONFLICT (maNCC) DO NOTHING;


-- ============================================================
-- 7. SAN PHAM MAU
-- Chi tao thong tin san pham va gia tham khao.
-- soLuongTon de 0 vi chua co phieu nhap kho nao.
-- ============================================================

-- Bổ sung danh mục Thú bông
INSERT INTO danh_muc_san_pham (tenDanhMuc, trangThai) VALUES ('Thu bong', 'HoatDong') ON CONFLICT (tenDanhMuc) DO NOTHING;

INSERT INTO san_pham
    (danhMucId, donViTinhId, maSP, tenSP, giaNhap, giaBan, soLuongTon, trangThai, imageUrl)
SELECT
    dm.id,
    dvt.id,
    p.maSP,
    p.tenSP,
    p.giaNhap,
    p.giaBan,
    p.soLuongTon,
    'KinhDoanh',
    p.imageUrl
FROM (
    VALUES
        -- 6 sản phẩm nguyên bản chuẩn Figma Screen 02 (Hình ảnh cắt trực tiếp từ thiết kế)
        ('Thu bong', 'Cai', 'GB001', 'Gấu bông Teddy 25cm', 90000::numeric, 180000::numeric, 24, '/products/teddy.png'),
        ('Qua tang', 'Cai', 'LY002', 'Ly sứ tặng Ba', 45000::numeric, 95000::numeric, 36, '/products/lysu.png'),
        ('Do trang tri', 'Cai', 'DN003', 'Đèn ngủ gỗ', 120000::numeric, 220000::numeric, 12, '/products/denngu.png'),
        ('Qua tang', 'Hop', 'HQ004', 'Hộp quà pastel', 20000::numeric, 45000::numeric, 58, '/products/hopqua.png'),
        ('Van phong pham', 'Bo', 'BT005', 'Bộ bút quà tặng', 60000::numeric, 120000::numeric, 18, '/products/but.png'),
        ('Do trang tri', 'Cai', 'DL006', 'Đèn lồng trang trí', 130000::numeric, 250000::numeric, 9, '/products/denlong.png'),

        -- Các mặt hàng lưu niệm truyền thống (Seed mẫu với ảnh chuẩn đẹp, tải cục bộ)
        ('Do luu niem', 'Cai', 'SP001', 'Nam châm tủ lạnh Việt Nam', 12000::numeric, 25000::numeric, 50, '/products/namcham.jpg'),
        ('Do luu niem', 'Cai', 'SP002', 'Mô hình xe xích lô mini', 45000::numeric, 85000::numeric, 30, '/products/xichlo.jpg'),
        ('Moc khoa', 'Cai', 'SP003', 'Móc khóa nón lá', 8000::numeric, 20000::numeric, 80, '/products/mockhoa.jpg'),
        ('Moc khoa', 'Cai', 'SP004', 'Móc khóa landmark', 10000::numeric, 25000::numeric, 65, '/products/mockhoa_landmark.jpg'),
        ('Do thu cong my nghe', 'Cai', 'SP005', 'Quạt giấy thủ công', 25000::numeric, 50000::numeric, 40, '/products/quatgiay.jpg'),
        ('Trang suc va phu kien', 'Bo', 'SP006', 'Bộ vòng tay handmade', 30000::numeric, 65000::numeric, 35, '/products/vongtay.jpg'),
        ('Van phong pham', 'Quyen', 'SP007', 'Sổ tay họa tiết Việt Nam', 18000::numeric, 40000::numeric, 55, '/products/sotay.jpg'),
        ('Do trang tri', 'Cai', 'SP008', 'Tranh trang trí mini', 35000::numeric, 75000::numeric, 25, '/products/denngu.png'),
        ('Ao thun va tui vai', 'Cai', 'SP009', 'Túi vải in họa tiết', 28000::numeric, 60000::numeric, 45, '/products/tuivai.jpg'),
        ('Qua tang', 'Hop', 'SP010', 'Hộp quà lưu niệm', 40000::numeric, 90000::numeric, 30, '/products/hopqua.png')
) AS p(tenDanhMuc, tenDonVi, maSP, tenSP, giaNhap, giaBan, soLuongTon, imageUrl)
JOIN danh_muc_san_pham dm ON dm.tenDanhMuc = p.tenDanhMuc
JOIN don_vi_tinh dvt ON dvt.tenDonVi = p.tenDonVi
ON CONFLICT (maSP) DO UPDATE
SET tenSP = EXCLUDED.tenSP,
    giaBan = EXCLUDED.giaBan,
    soLuongTon = EXCLUDED.soLuongTon,
    imageUrl = EXCLUDED.imageUrl,
    danhMucId = EXCLUDED.danhMucId,
    donViTinhId = EXCLUDED.donViTinhId;


-- ============================================================
-- 10. MA GIAM GIA (VOUCHERS)
-- ============================================================
INSERT INTO MA_GIAM_GIA (maCode, tenVoucher, loaiGiamGia, giaTriGiam, ngayBatDau, ngayKetThuc)
VALUES
    ('QUATANG10', 'Giảm giá 10% Quà Tặng', 'PhanTram', 10.00, CURRENT_TIMESTAMP - INTERVAL '30 days', CURRENT_TIMESTAMP + INTERVAL '90 days'),
    ('GIAM10K', 'Giảm 10.000 đ đơn hàng', 'SoTien', 10000.00, CURRENT_TIMESTAMP - INTERVAL '30 days', CURRENT_TIMESTAMP + INTERVAL '90 days'),
    ('TRIAN50K', 'Tri ân khách hàng giảm 50.000 đ', 'SoTien', 50000.00, CURRENT_TIMESTAMP - INTERVAL '30 days', CURRENT_TIMESTAMP + INTERVAL '90 days'),
    ('HETHAN', 'Mã ưu đãi đã hết hạn', 'PhanTram', 20.00, CURRENT_TIMESTAMP - INTERVAL '30 days', CURRENT_TIMESTAMP - INTERVAL '5 days')
ON CONFLICT (maCode) DO UPDATE
SET tenVoucher = EXCLUDED.tenVoucher,
    loaiGiamGia = EXCLUDED.loaiGiamGia,
    giaTriGiam = EXCLUDED.giaTriGiam,
    ngayBatDau = EXCLUDED.ngayBatDau,
    ngayKetThuc = EXCLUDED.ngayKetThuc;

-- ============================================================
-- KET THUC SEED DATA
-- ============================================================