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

INSERT INTO san_pham
    (danhMucId, donViTinhId, maSP, tenSP, giaNhap, giaBan, soLuongTon, trangThai)
SELECT
    dm.id,
    dvt.id,
    p.maSP,
    p.tenSP,
    p.giaNhap,
    p.giaBan,
    0,
    'KinhDoanh'
FROM (
    VALUES
        ('Do luu niem', 'Cai', 'SP001', 'Nam cham tu lanh Viet Nam', 12000::numeric, 25000::numeric),
        ('Do luu niem', 'Cai', 'SP002', 'Mo hinh xe xich lo mini', 45000::numeric, 85000::numeric),
        ('Moc khoa', 'Cai', 'SP003', 'Moc khoa non la', 8000::numeric, 20000::numeric),
        ('Moc khoa', 'Cai', 'SP004', 'Moc khoa landmark', 10000::numeric, 25000::numeric),
        ('Do thu cong my nghe', 'Cai', 'SP005', 'Quat giay thu cong', 25000::numeric, 50000::numeric),
        ('Trang suc va phu kien', 'Bo', 'SP006', 'Bo vong tay handmade', 30000::numeric, 65000::numeric),
        ('Van phong pham', 'Quyen', 'SP007', 'So tay hoa tiet Viet Nam', 18000::numeric, 40000::numeric),
        ('Do trang tri', 'Cai', 'SP008', 'Tranh trang tri mini', 35000::numeric, 75000::numeric),
        ('Ao thun va tui vai', 'Cai', 'SP009', 'Tui vai in hoa tiet', 28000::numeric, 60000::numeric),
        ('Qua tang', 'Hop', 'SP010', 'Hop qua luu niem', 40000::numeric, 90000::numeric)
) AS p(tenDanhMuc, tenDonVi, maSP, tenSP, giaNhap, giaBan)
JOIN danh_muc_san_pham dm ON dm.tenDanhMuc = p.tenDanhMuc
JOIN don_vi_tinh dvt ON dvt.tenDonVi = p.tenDonVi
ON CONFLICT (maSP) DO NOTHING;


-- ============================================================
-- KET THUC SEED DATA
-- ============================================================