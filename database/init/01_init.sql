-- Tạo cơ sở dữ liệu (chạy riêng lệnh này nếu chưa tạo DB)
-- CREATE DATABASE quan_ly_cua_hang_luu_niem;

-- =====================================================================
-- A. BẢNG THAM SỐ HỆ THỐNG (Đảm bảo tính tiến hóa)
-- =====================================================================
CREATE TABLE THAM_SO (
    tenThamSo VARCHAR(100) PRIMARY KEY,
    kieuDuLieu VARCHAR(20) NOT NULL,
    giaTri VARCHAR(255) NOT NULL,
    moTa VARCHAR(500) NULL
);

-- =====================================================================
-- B. CÁC BẢNG DANH MỤC (Đảm bảo tính tiến hóa)
-- =====================================================================
CREATE TABLE VAI_TRO (
    id INT GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    tenVaiTro VARCHAR(50) NOT NULL UNIQUE,
    moTa VARCHAR(255) NULL
);

CREATE TABLE DANH_MUC_SAN_PHAM (
    id INT GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    tenDanhMuc VARCHAR(100) NOT NULL UNIQUE,
    trangThai VARCHAR(50) NOT NULL DEFAULT 'HoatDong'
);

CREATE TABLE DON_VI_TINH (
    id INT GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    tenDonVi VARCHAR(50) NOT NULL UNIQUE
);

CREATE TABLE HINH_THUC_THANH_TOAN (
    id INT GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    tenHinhThuc VARCHAR(100) NOT NULL UNIQUE,
    trangThai VARCHAR(50) NOT NULL DEFAULT 'HoatDong'
);

-- =====================================================================
-- C. CÁC BẢNG NGHIỆP VỤ CHÍNH
-- =====================================================================

-- 6. Bảng Người dùng (Nhân viên & Quản lý)
CREATE TABLE NGUOI_DUNG (
    id INT GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    vaiTroId INT NOT NULL,
    tenDangNhap VARCHAR(50) NOT NULL UNIQUE,
    matKhau VARCHAR(255) NOT NULL,
    hoTen VARCHAR(100) NOT NULL,
    soDienThoai CHAR(10) NOT NULL,
    trangThai VARCHAR(50) NOT NULL DEFAULT 'HoatDong',
    CONSTRAINT FK_NguoiDung_VaiTro FOREIGN KEY (vaiTroId) REFERENCES VAI_TRO(id)
);

-- 7. Bảng Nhà cung cấp
CREATE TABLE NHA_CUNG_CAP (
    id INT GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    maNCC VARCHAR(50) NOT NULL UNIQUE,
    tenNCC VARCHAR(150) NOT NULL,
    soDienThoai VARCHAR(15) NOT NULL,
    diaChi VARCHAR(255) NULL
);

-- 8. Bảng Sản phẩm
CREATE TABLE SAN_PHAM (
    id INT GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    danhMucId INT NOT NULL,
    donViTinhId INT NOT NULL,
    maSP VARCHAR(50) NOT NULL UNIQUE,
    imageUrl varchar(255) NULL,
    tenSP VARCHAR(150) NOT NULL,
    giaNhap NUMERIC(18, 2) NOT NULL CHECK (giaNhap >= 0),
    giaBan NUMERIC(18, 2) NOT NULL CHECK (giaBan >= 0),
    soLuongTon INT NOT NULL DEFAULT 0 CHECK (soLuongTon >= 0),
    trangThai VARCHAR(50) NOT NULL DEFAULT 'KinhDoanh',
    CONSTRAINT FK_SanPham_DanhMuc FOREIGN KEY (danhMucId) REFERENCES DANH_MUC_SAN_PHAM(id),
    CONSTRAINT FK_SanPham_DonViTinh FOREIGN KEY (donViTinhId) REFERENCES DON_VI_TINH(id)
);

-- 9. Bảng Khách hàng
CREATE TABLE KHACH_HANG (
    id INT GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    soDienThoai CHAR(10) NOT NULL UNIQUE,
    tenKhachHang VARCHAR(100) NOT NULL,
    diemTichLuy INT NOT NULL DEFAULT 0 CHECK (diemTichLuy >= 0)
);

-- 10. Bảng Mã giảm giá
CREATE TABLE MA_GIAM_GIA (
    id INT GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    maCode VARCHAR(50) NOT NULL UNIQUE,
    tenVoucher VARCHAR(150) NOT NULL,
    loaiGiamGia VARCHAR(50) NOT NULL, -- 'PhanTram' hoặc 'SoTien'
    giaTriGiam NUMERIC(18, 2) NOT NULL CHECK (giaTriGiam > 0),
    ngayBatDau TIMESTAMP NOT NULL,
    ngayKetThuc TIMESTAMP NOT NULL
);

-- 11. Bảng Ca làm việc
CREATE TABLE CA_LAM_VIEC (
    id INT GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    nguoiDungId INT NOT NULL,
    thoiGianMoCa TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    thoiGianChotCa TIMESTAMP NULL,
    tienMatBanGiao NUMERIC(18, 2) NOT NULL CHECK (tienMatBanGiao >= 0),
    tienMatThucTe NUMERIC(18, 2) NULL CHECK (tienMatThucTe >= 0),
    CONSTRAINT FK_CaLamViec_NguoiDung FOREIGN KEY (nguoiDungId) REFERENCES NGUOI_DUNG(id)
);

-- 12. Bảng Chấm công (Quan hệ 1-1 với Ca làm việc)
CREATE TABLE CHAM_CONG (
    id INT GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    nguoiDungId INT NOT NULL,
    caLamViecId INT NOT NULL UNIQUE,
    gioCheckIn TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    gioCheckOut TIMESTAMP NULL,
    CONSTRAINT FK_ChamCong_NguoiDung FOREIGN KEY (nguoiDungId) REFERENCES NGUOI_DUNG(id),
    CONSTRAINT FK_ChamCong_CaLamViec FOREIGN KEY (caLamViecId) REFERENCES CA_LAM_VIEC(id)
);

-- 13. Bảng Đơn hàng
CREATE TABLE DON_HANG (
    id INT GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    caLamViecId INT NOT NULL,
    khachHangId INT NULL, -- Cho phép NULL nếu là khách vãng lai
    nguoiDungId INT NOT NULL,
    maGiamGiaId INT NULL, -- Cho phép NULL nếu không dùng voucher
    hinhThucThanhToanId INT NOT NULL,
    ngayTao TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    tongTienThanhToan NUMERIC(18, 2) NOT NULL CHECK (tongTienThanhToan >= 0),
    trangThai VARCHAR(50) NOT NULL DEFAULT 'DangLapDon',
    CONSTRAINT FK_DonHang_CaLamViec FOREIGN KEY (caLamViecId) REFERENCES CA_LAM_VIEC(id),
    CONSTRAINT FK_DonHang_KhachHang FOREIGN KEY (khachHangId) REFERENCES KHACH_HANG(id),
    CONSTRAINT FK_DonHang_NguoiDung FOREIGN KEY (nguoiDungId) REFERENCES NGUOI_DUNG(id),
    CONSTRAINT FK_DonHang_MaGiamGia FOREIGN KEY (maGiamGiaId) REFERENCES MA_GIAM_GIA(id),
    CONSTRAINT FK_DonHang_HinhThucTT FOREIGN KEY (hinhThucThanhToanId) REFERENCES HINH_THUC_THANH_TOAN(id)
);

-- 14. Bảng Chi tiết đơn hàng
CREATE TABLE CHI_TIET_DON_HANG (
    donHangId INT NOT NULL,
    sanPhamId INT NOT NULL,
    soLuong INT NOT NULL CHECK (soLuong > 0),
    donGia NUMERIC(18, 2) NOT NULL CHECK (donGia >= 0),
    thanhTien NUMERIC(18, 2) NOT NULL CHECK (thanhTien >= 0),
    PRIMARY KEY (donHangId, sanPhamId),
    CONSTRAINT FK_CTDH_DonHang FOREIGN KEY (donHangId) REFERENCES DON_HANG(id),
    CONSTRAINT FK_CTDH_SanPham FOREIGN KEY (sanPhamId) REFERENCES SAN_PHAM(id)
);

-- 15. Bảng Phiếu nhập kho
CREATE TABLE PHIEU_NHAP_KHO (
    id INT GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    maPhieu VARCHAR(50) NOT NULL UNIQUE,
    nhaCungCapId INT NOT NULL,
    nguoiDungId INT NOT NULL,
    tongTienNhap NUMERIC(18, 2) NOT NULL CHECK (tongTienNhap >= 0),
    ngayNhap TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    CONSTRAINT FK_PNK_NhaCungCap FOREIGN KEY (nhaCungCapId) REFERENCES NHA_CUNG_CAP(id),
    CONSTRAINT FK_PNK_NguoiDung FOREIGN KEY (nguoiDungId) REFERENCES NGUOI_DUNG(id)
);

-- 16. Bảng Chi tiết nhập kho
CREATE TABLE CHI_TIET_NHAP_KHO (
    phieuNhapKhoId INT NOT NULL,
    sanPhamId INT NOT NULL,
    soLuongNhap INT NOT NULL CHECK (soLuongNhap > 0),
    donGiaNhap NUMERIC(18, 2) NOT NULL CHECK (donGiaNhap >= 0),
    thanhTien NUMERIC(18, 2) NOT NULL CHECK (thanhTien >= 0),
    PRIMARY KEY (phieuNhapKhoId, sanPhamId),
    CONSTRAINT FK_CTNK_PhieuNhapKho FOREIGN KEY (phieuNhapKhoId) REFERENCES PHIEU_NHAP_KHO(id),
    CONSTRAINT FK_CTNK_SanPham FOREIGN KEY (sanPhamId) REFERENCES SAN_PHAM(id)
);

-- 17. Bảng Phiếu kiểm kê
CREATE TABLE PHIEU_KIEM_KE (
    id INT GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    maPhieu VARCHAR(50) NOT NULL UNIQUE,
    nguoiDungId INT NOT NULL,
    ngayKiemKe TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    trangThai VARCHAR(50) NOT NULL DEFAULT 'DangKiemKe',
    CONSTRAINT FK_PKK_NguoiDung FOREIGN KEY (nguoiDungId) REFERENCES NGUOI_DUNG(id)
);

-- 18. Bảng Chi tiết kiểm kê
CREATE TABLE CHI_TIET_KIEM_KE (
    phieuKiemKeId INT NOT NULL,
    sanPhamId INT NOT NULL,
    soTonSoSach INT NOT NULL CHECK (soTonSoSach >= 0),
    soTonThucTe INT NOT NULL CHECK (soTonThucTe >= 0),
    soLuongChenhLech INT NOT NULL,
    lyDoChenhLech VARCHAR(255) NULL,
    PRIMARY KEY (phieuKiemKeId, sanPhamId),
    CONSTRAINT FK_CTKK_PhieuKiemKe FOREIGN KEY (phieuKiemKeId) REFERENCES PHIEU_KIEM_KE(id),
    CONSTRAINT FK_CTKK_SanPham FOREIGN KEY (sanPhamId) REFERENCES SAN_PHAM(id)
);

-- 19. Bảng Phiếu trả hàng
CREATE TABLE PHIEU_TRA_HANG (
    id INT GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    maPhieu VARCHAR(50) NOT NULL UNIQUE,
    donHangGocId INT NOT NULL,
    nguoiDungId INT NOT NULL,
    tongTienHoan NUMERIC(18, 2) NOT NULL CHECK (tongTienHoan >= 0),
    lyDoTra VARCHAR(255) NOT NULL,
    ngayTra TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    CONSTRAINT FK_PTH_DonHang FOREIGN KEY (donHangGocId) REFERENCES DON_HANG(id),
    CONSTRAINT FK_PTH_NguoiDung FOREIGN KEY (nguoiDungId) REFERENCES NGUOI_DUNG(id)
);

-- 20. Bảng Chi tiết trả hàng
CREATE TABLE CHI_TIET_TRA_HANG (
    phieuTraHangId INT NOT NULL,
    sanPhamId INT NOT NULL,
    soLuongTra INT NOT NULL CHECK (soLuongTra > 0),
    donGiaHoan NUMERIC(18, 2) NOT NULL CHECK (donGiaHoan >= 0),
    tinhTrangHang VARCHAR(255) NOT NULL,
    PRIMARY KEY (phieuTraHangId, sanPhamId),
    CONSTRAINT FK_CTTH_PhieuTraHang FOREIGN KEY (phieuTraHangId) REFERENCES PHIEU_TRA_HANG(id),
    CONSTRAINT FK_CTTH_SanPham FOREIGN KEY (sanPhamId) REFERENCES SAN_PHAM(id)
);

-- =====================================================================
-- D. DỮ LIỆU KHỞI TẠO MẪU CHO BẢNG THAM SỐ & DANH MỤC
-- =====================================================================
INSERT INTO THAM_SO (tenThamSo, kieuDuLieu, giaTri, moTa) VALUES
('SoLanDangNhapSaiToiDa', 'int', '5', 'Khóa tài khoản tạm thời khi nhập sai mật khẩu liên tiếp'),
('SoNgayChoPhepDoiTra', 'int', '7', 'Số ngày tối đa cho phép khách đổi trả hàng tính từ ngày mua'),
('TyLeTichDiem', 'float', '0.01', 'Tỷ lệ tích điểm thưởng theo tổng giá trị hóa đơn (1%)'),
('TyLeDoiDiem', 'float', '1000', 'Giá trị quy đổi 1 điểm thưởng thành tiền giảm giá (1000 VNĐ)');

INSERT INTO VAI_TRO (tenVaiTro, moTa) VALUES
('QuanLy', 'Toàn quyền quản trị hệ thống, kho hàng, báo cáo và phân quyền'),
('NhanVien', 'Nhân viên thu ngân thực hiện bán hàng, mở/chốt ca và chấm công');

INSERT INTO DON_VI_TINH (tenDonVi) VALUES ('Cái'), ('Hộp'), ('Bộ'), ('Con'), ('Móc khóa');

INSERT INTO HINH_THUC_THANH_TOAN (tenHinhThuc) VALUES ('Tiền mặt'), ('Chuyển khoản QR');
