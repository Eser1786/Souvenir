-- =====================================================================
-- Souvenir POS - Khởi tạo cơ sở dữ liệu PostgreSQL
-- Nguồn: ERD + Sơ đồ trạng thái (Đơn hàng, Ca làm việc) trong Đồ_án_OOAD.docx
-- File này được Docker tự chạy ở lần khởi tạo volume đầu tiên
-- (mount vào /docker-entrypoint-initdb.d/).
-- Quy ước: bảng/cột snake_case, khóa chính "id" kiểu INT identity.
-- =====================================================================

BEGIN;

-- ---------------------------------------------------------------------
-- 1. ENUM / DOMAIN
-- ---------------------------------------------------------------------
-- Dùng VARCHAR + CHECK (thay vì ENUM của Postgres) để EF Core map dễ
-- và dễ thêm giá trị mới về sau.

-- ---------------------------------------------------------------------
-- 2. NGƯỜI DÙNG
-- ---------------------------------------------------------------------
CREATE TABLE nguoi_dung (
    id              INT GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    ten_dang_nhap   VARCHAR(50)  NOT NULL UNIQUE,
    mat_khau        VARCHAR(255) NOT NULL,              -- lưu HASH (BCrypt), không lưu mật khẩu thô
    ho_ten          VARCHAR(100) NOT NULL,
    so_dien_thoai   VARCHAR(15),
    vai_tro         VARCHAR(20)  NOT NULL DEFAULT 'NHAN_VIEN',
    trang_thai      VARCHAR(20)  NOT NULL DEFAULT 'HOAT_DONG',
    CONSTRAINT ck_nguoi_dung_vai_tro   CHECK (vai_tro IN ('QUAN_LY', 'NHAN_VIEN', 'THU_KHO')),
    CONSTRAINT ck_nguoi_dung_trang_thai CHECK (trang_thai IN ('HOAT_DONG', 'KHOA'))
);

-- ---------------------------------------------------------------------
-- 3. DANH MỤC: NHÀ CUNG CẤP, SẢN PHẨM, KHÁCH HÀNG, MÃ GIẢM GIÁ
-- ---------------------------------------------------------------------
CREATE TABLE nha_cung_cap (
    id              INT GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    ma_ncc          VARCHAR(20)  NOT NULL UNIQUE,
    ten_ncc         VARCHAR(150) NOT NULL,
    so_dien_thoai   VARCHAR(15),
    dia_chi         VARCHAR(255)
);

CREATE TABLE san_pham (
    id              INT GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    ma_sp           VARCHAR(50)  NOT NULL UNIQUE,       -- mã vạch / mã sản phẩm
    ten_sp          VARCHAR(200) NOT NULL,
    don_vi_tinh     VARCHAR(30)  NOT NULL DEFAULT 'Cái',
    gia_nhap        NUMERIC(18,2) NOT NULL DEFAULT 0 CHECK (gia_nhap >= 0),
    gia_ban         NUMERIC(18,2) NOT NULL DEFAULT 0 CHECK (gia_ban  >= 0),
    so_luong_ton    INT           NOT NULL DEFAULT 0 CHECK (so_luong_ton >= 0)
);

CREATE TABLE khach_hang (
    id              INT GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    so_dien_thoai   VARCHAR(15)  NOT NULL UNIQUE,
    ten_khach_hang  VARCHAR(100) NOT NULL,
    diem_tich_luy   INT          NOT NULL DEFAULT 0 CHECK (diem_tich_luy >= 0)
);

CREATE TABLE ma_giam_gia (
    id              INT GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    ma_code         VARCHAR(30)  NOT NULL UNIQUE,
    ten_voucher     VARCHAR(150) NOT NULL,
    gia_tri_giam    NUMERIC(18,2) NOT NULL CHECK (gia_tri_giam > 0),
    ngay_bat_dau    TIMESTAMP    NOT NULL,
    ngay_ket_thuc   TIMESTAMP    NOT NULL,
    CONSTRAINT ck_ma_giam_gia_ngay CHECK (ngay_ket_thuc >= ngay_bat_dau)
);

-- ---------------------------------------------------------------------
-- 4. CA LÀM VIỆC & CHẤM CÔNG
-- Trạng thái theo sơ đồ: CHUA_MO -> DANG_MO -> (CHO_XU_LY_CHENH_LECH) -> DA_CHOT
-- ---------------------------------------------------------------------
CREATE TABLE ca_lam_viec (
    id                  INT GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    nguoi_dung_id       INT NOT NULL REFERENCES nguoi_dung(id),
    thoi_gian_mo_ca     TIMESTAMP,
    thoi_gian_chot_ca   TIMESTAMP,
    tien_mat_ban_giao   NUMERIC(18,2) NOT NULL DEFAULT 0 CHECK (tien_mat_ban_giao >= 0),
    tien_mat_thuc_te    NUMERIC(18,2) CHECK (tien_mat_thuc_te >= 0),
    ly_do_chenh_lech    VARCHAR(500),                   -- lý do giải trình khi lệch tiền
    trang_thai          VARCHAR(30) NOT NULL DEFAULT 'CHUA_MO',   -- bổ sung từ sơ đồ trạng thái
    CONSTRAINT ck_ca_trang_thai CHECK (trang_thai IN
        ('CHUA_MO', 'DANG_MO', 'CHO_XU_LY_CHENH_LECH', 'DA_CHOT')),
    CONSTRAINT ck_ca_thoi_gian CHECK (thoi_gian_chot_ca IS NULL
        OR thoi_gian_mo_ca IS NULL OR thoi_gian_chot_ca >= thoi_gian_mo_ca)
);

-- Mỗi nhân viên chỉ có tối đa 1 ca đang mở tại một thời điểm
CREATE UNIQUE INDEX ux_ca_dang_mo_moi_nguoi
    ON ca_lam_viec (nguoi_dung_id) WHERE trang_thai = 'DANG_MO';

CREATE TABLE cham_cong (
    id              INT GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    nguoi_dung_id   INT NOT NULL REFERENCES nguoi_dung(id),
    ca_lam_viec_id  INT NOT NULL REFERENCES ca_lam_viec(id),
    gio_check_in    TIMESTAMP NOT NULL,
    gio_check_out   TIMESTAMP,
    CONSTRAINT ck_cham_cong_gio CHECK (gio_check_out IS NULL OR gio_check_out >= gio_check_in)
);

-- ---------------------------------------------------------------------
-- 5. ĐƠN HÀNG
-- Trạng thái theo sơ đồ: DANG_LAP -> LUU_NHAP / CHO_THANH_TOAN / DA_HUY
--                        CHO_THANH_TOAN -> DA_THANH_TOAN / DANG_LAP (giao dịch thất bại)
--                        DA_THANH_TOAN -> DA_TRA_HANG
-- ---------------------------------------------------------------------
CREATE TABLE don_hang (
    id                  INT GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    ca_lam_viec_id      INT NOT NULL REFERENCES ca_lam_viec(id),
    khach_hang_id       INT REFERENCES khach_hang(id),          -- khách vãng lai: NULL
    nguoi_dung_id       INT NOT NULL REFERENCES nguoi_dung(id), -- nhân viên tạo đơn
    ma_giam_gia_id      INT REFERENCES ma_giam_gia(id),
    ngay_tao            TIMESTAMP NOT NULL DEFAULT NOW(),
    tong_tien_thanh_toan NUMERIC(18,2) NOT NULL DEFAULT 0 CHECK (tong_tien_thanh_toan >= 0),
    hinh_thuc_thanh_toan VARCHAR(20),                           -- NULL cho tới khi thanh toán
    trang_thai          VARCHAR(30) NOT NULL DEFAULT 'DANG_LAP', -- bổ sung từ sơ đồ trạng thái
    CONSTRAINT ck_don_hang_trang_thai CHECK (trang_thai IN
        ('DANG_LAP', 'LUU_NHAP', 'CHO_THANH_TOAN', 'DA_THANH_TOAN', 'DA_HUY', 'DA_TRA_HANG')),
    CONSTRAINT ck_don_hang_hinh_thuc CHECK (hinh_thuc_thanh_toan IS NULL
        OR hinh_thuc_thanh_toan IN ('TIEN_MAT', 'QR'))
);

CREATE TABLE chi_tiet_don_hang (
    don_hang_id     INT NOT NULL REFERENCES don_hang(id) ON DELETE CASCADE,
    san_pham_id     INT NOT NULL REFERENCES san_pham(id),
    so_luong        INT NOT NULL CHECK (so_luong > 0),
    don_gia         NUMERIC(18,2) NOT NULL CHECK (don_gia >= 0),
    thanh_tien      NUMERIC(18,2) GENERATED ALWAYS AS (so_luong * don_gia) STORED,
    PRIMARY KEY (don_hang_id, san_pham_id)
);

-- ---------------------------------------------------------------------
-- 6. TRẢ HÀNG
-- ---------------------------------------------------------------------
CREATE TABLE phieu_tra_hang (
    id              INT GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    ma_phieu        VARCHAR(30) NOT NULL UNIQUE,
    don_hang_goc_id INT NOT NULL REFERENCES don_hang(id),
    nguoi_dung_id   INT NOT NULL REFERENCES nguoi_dung(id),    -- quản lý xác nhận hoàn tiền
    tong_tien_hoan  NUMERIC(18,2) NOT NULL DEFAULT 0 CHECK (tong_tien_hoan >= 0),
    ngay_tra        TIMESTAMP NOT NULL DEFAULT NOW()
);

CREATE TABLE chi_tiet_tra_hang (
    phieu_tra_hang_id INT NOT NULL REFERENCES phieu_tra_hang(id) ON DELETE CASCADE,
    san_pham_id       INT NOT NULL REFERENCES san_pham(id),
    so_luong_tra      INT NOT NULL CHECK (so_luong_tra > 0),
    don_gia_hoan      NUMERIC(18,2) NOT NULL CHECK (don_gia_hoan >= 0),
    tinh_trang_hang   VARCHAR(100),                             -- vd: còn nguyên, lỗi, hư hỏng
    PRIMARY KEY (phieu_tra_hang_id, san_pham_id)
);

-- ---------------------------------------------------------------------
-- 7. NHẬP KHO
-- ---------------------------------------------------------------------
CREATE TABLE phieu_nhap_kho (
    id                INT GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    ma_phieu          VARCHAR(30) NOT NULL UNIQUE,
    nha_cung_cap_id   INT NOT NULL REFERENCES nha_cung_cap(id),
    nguoi_dung_id     INT NOT NULL REFERENCES nguoi_dung(id),
    tong_tien_nhap    NUMERIC(18,2) NOT NULL DEFAULT 0 CHECK (tong_tien_nhap >= 0),
    ngay_nhap         TIMESTAMP NOT NULL DEFAULT NOW()
);

CREATE TABLE chi_tiet_nhap_kho (
    phieu_nhap_kho_id INT NOT NULL REFERENCES phieu_nhap_kho(id) ON DELETE CASCADE,
    san_pham_id       INT NOT NULL REFERENCES san_pham(id),
    so_luong_nhap     INT NOT NULL CHECK (so_luong_nhap > 0),
    don_gia_nhap      NUMERIC(18,2) NOT NULL CHECK (don_gia_nhap >= 0),
    thanh_tien        NUMERIC(18,2) GENERATED ALWAYS AS (so_luong_nhap * don_gia_nhap) STORED,
    PRIMARY KEY (phieu_nhap_kho_id, san_pham_id)
);

-- ---------------------------------------------------------------------
-- 8. KIỂM KÊ
-- ---------------------------------------------------------------------
CREATE TABLE phieu_kiem_ke (
    id              INT GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    ma_phieu        VARCHAR(30) NOT NULL UNIQUE,
    nguoi_dung_id   INT NOT NULL REFERENCES nguoi_dung(id),
    ngay_kiem_ke    TIMESTAMP NOT NULL DEFAULT NOW(),
    trang_thai      VARCHAR(20) NOT NULL DEFAULT 'DANG_KIEM',
    CONSTRAINT ck_kiem_ke_trang_thai CHECK (trang_thai IN ('DANG_KIEM', 'HOAN_TAT', 'HUY'))
);

CREATE TABLE chi_tiet_kiem_ke (
    phieu_kiem_ke_id    INT NOT NULL REFERENCES phieu_kiem_ke(id) ON DELETE CASCADE,
    san_pham_id         INT NOT NULL REFERENCES san_pham(id),
    so_ton_so_sach      INT NOT NULL CHECK (so_ton_so_sach >= 0),
    so_ton_thuc_te      INT NOT NULL CHECK (so_ton_thuc_te >= 0),
    so_luong_chenh_lech INT GENERATED ALWAYS AS (so_ton_thuc_te - so_ton_so_sach) STORED,
    PRIMARY KEY (phieu_kiem_ke_id, san_pham_id)
);

-- ---------------------------------------------------------------------
-- 9. INDEX hỗ trợ truy vấn thường dùng
-- ---------------------------------------------------------------------
CREATE INDEX ix_san_pham_ten            ON san_pham (ten_sp);
CREATE INDEX ix_don_hang_ca             ON don_hang (ca_lam_viec_id);
CREATE INDEX ix_don_hang_khach          ON don_hang (khach_hang_id);
CREATE INDEX ix_don_hang_ngay_tao       ON don_hang (ngay_tao);
CREATE INDEX ix_don_hang_trang_thai     ON don_hang (trang_thai);
CREATE INDEX ix_chi_tiet_don_hang_sp    ON chi_tiet_don_hang (san_pham_id);
CREATE INDEX ix_ca_lam_viec_nguoi_dung  ON ca_lam_viec (nguoi_dung_id);
CREATE INDEX ix_cham_cong_ca            ON cham_cong (ca_lam_viec_id);
CREATE INDEX ix_phieu_tra_hang_don      ON phieu_tra_hang (don_hang_goc_id);
CREATE INDEX ix_phieu_nhap_ncc          ON phieu_nhap_kho (nha_cung_cap_id);

-- ---------------------------------------------------------------------
-- 10. DỮ LIỆU MẪU (chỉ dùng để dev/demo)
-- ---------------------------------------------------------------------
-- LƯU Ý: cột mat_khau là placeholder. Hãy thay bằng BCrypt hash thật
-- (vd: tạo bằng BCrypt.Net trong DbSeeder của Souvenir.Infrastructure).
INSERT INTO nguoi_dung (ten_dang_nhap, mat_khau, ho_ten, so_dien_thoai, vai_tro) VALUES
    ('admin',  'CHANGE_ME_BCRYPT_HASH', 'Quản lý cửa hàng', '0900000001', 'QUAN_LY'),
    ('nv01',   'CHANGE_ME_BCRYPT_HASH', 'Nhân viên thu ngân 01', '0900000002', 'NHAN_VIEN'),
    ('kho01',  'CHANGE_ME_BCRYPT_HASH', 'Nhân viên kho 01', '0900000003', 'THU_KHO');

INSERT INTO nha_cung_cap (ma_ncc, ten_ncc, so_dien_thoai, dia_chi) VALUES
    ('NCC001', 'Làng nghề Bát Tràng', '0911111111', 'Gia Lâm, Hà Nội'),
    ('NCC002', 'Xưởng thủ công mỹ nghệ Huế', '0922222222', 'TP. Huế');

INSERT INTO san_pham (ma_sp, ten_sp, don_vi_tinh, gia_nhap, gia_ban, so_luong_ton) VALUES
    ('SP001', 'Móc khóa gốm',          'Cái', 15000,  35000, 100),
    ('SP002', 'Nón lá mini',           'Cái', 20000,  50000,  60),
    ('SP003', 'Tranh thêu tay',        'Bức', 150000, 320000, 15),
    ('SP004', 'Ly sứ in hình phố cổ',  'Cái', 40000,  95000,  40);

INSERT INTO khach_hang (so_dien_thoai, ten_khach_hang, diem_tich_luy) VALUES
    ('0987654321', 'Nguyễn Văn A', 120),
    ('0976543210', 'Trần Thị B',    30);

INSERT INTO ma_giam_gia (ma_code, ten_voucher, gia_tri_giam, ngay_bat_dau, ngay_ket_thuc) VALUES
    ('GIAM10K', 'Giảm 10.000đ',  10000, NOW() - INTERVAL '1 day', NOW() + INTERVAL '30 days'),
    ('GIAM50K', 'Giảm 50.000đ',  50000, NOW() - INTERVAL '1 day', NOW() + INTERVAL '30 days');

COMMIT;