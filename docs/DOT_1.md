# 🚀 ĐỢT 1: BẮT ĐẦU NGAY – XÂY DỰNG CÁC PHÂN HỆ ĐỘC LẬP
> **Thời gian thực hiện**: Chiều Ngày 2 – Hết Ngày 3  
> **Mục tiêu**: Cả 6 thành viên cùng làm việc song song, không ai phải chờ đợi ai. Hoàn thiện toàn bộ các tính năng nền tảng và dữ liệu đầu vào.

---

### 1. HƯỚNG DẪN BẮT ĐẦU (DÀNH CHO TẤT CẢ THÀNH VIÊN)
1. Kéo code mới nhất đã có sẵn Auth JWT và khung sườn POS:
   ```bash
   git pull origin dev
   git checkout -b feat/<ten-task-cua-ban>
   ```
2. Tài khoản test đăng nhập có sẵn trong Database:
   * **Quản lý (Admin)**: `admin` / `admin123`
   * **Nhân viên (Staff)**: `tranthuha` / `password123`
3. Mỗi người tự tạo thư mục riêng trong `frontend/src/components/`, không sửa đè file của nhau.

---

### 2. PHÂN CÔNG NHIỆM VỤ CHI TIẾT ĐỢT 1

#### 👤 Người 4: CRUD Sản phẩm & Phiếu nhập kho (`UC11`, `UC12`)
*Đã có sẵn Entity `SanPham` và API `GET /api/products` (cột ảnh trong DB là `imageUrl`, Entity C# là `HinhAnh`).*
* **Backend**:
  - [ ] Viết API Thêm sản phẩm mới: `POST /api/products` (nhận `imageUrl` / map vào `HinhAnh`).
  - [ ] Viết API Cập nhật sản phẩm: `PUT /api/products/{id}`.
  - [ ] Viết API Xóa sản phẩm: `DELETE /api/products/{id}`.
  - [ ] Tạo Entity & API `PhieuNhapKho` (khi nhập kho thì **cộng thêm vào tồn kho `soLuongTon`**).
* **Frontend**:
  - [ ] Tạo trang `ProductManagementScreen.tsx`: Bảng danh sách sản phẩm, modal Thêm/Sửa (có ô paste URL ảnh và preview trực tiếp).
  - [ ] Tạo trang `GoodsReceiptScreen.tsx`: Form tạo phiếu nhập hàng từ Nhà cung cấp.

#### 👤 Người 5: Mở ca làm việc & Chấm công (`UC03`, `UC02`)
*Cần làm trước để đơn hàng của POS có `ShiftId`.*
* **Backend**:
  - [ ] Tạo Entity `CaLamViec`: Viết API Mở ca `POST /api/shifts/open` (nhập số tiền mặt đầu ca trong két).
  - [ ] Tạo Entity `ChamCong`: Viết API Check-in / Check-out theo ngày cho nhân viên.
* **Frontend**:
  - [ ] Tạo Popup **Mở ca**: Hiển thị popup bắt buộc nhập tiền đầu ca khi nhân viên vừa đăng nhập vào POS mà chưa có ca mở.
  - [ ] Tạo nút bấm Chấm công nhanh trên Sidebar.

#### 👤 Người 6: Quản lý Khách hàng (`UC09`)
*Chuẩn bị dữ liệu khách hàng để nhúng vào màn hình POS.*
* **Backend**:
  - [ ] Tạo Entity `KhachHang`: Viết CRUD khách hàng.
  - [ ] Viết API tìm kiếm khách hàng bằng số điện thoại: `GET /api/customers?phone=...`.
* **Frontend**:
  - [ ] Tạo trang `CustomerManagementScreen.tsx`: Bảng danh sách khách hàng, thêm/sửa thông tin khách.
  - [ ] Hỗ trợ Người 2 kết nối ô tìm SĐT trên POS vào API tìm kiếm thật.

#### 👤 Người 1 (Lead): Quản lý Nhân sự & Phân quyền (`UC15`)
* **Backend**:
  - [ ] Viết API CRUD Nhân viên: `GET/POST/PUT/DELETE /api/users`.
  - [ ] Viết API Khóa / Mở khóa tài khoản nhân viên (`trangThai = 'Khoa'`).
  - [ ] Gắn quyền: Chỉ role `QuanLy` mới được truy cập các endpoint này.
* **Frontend**:
  - [ ] Tạo trang `UsersManagementScreen.tsx`: Bảng danh sách tài khoản, modal cấp tài khoản mới.
* **Nhiệm vụ Lead**: Review code, merge Pull Request của 5 bạn vào nhánh `dev`.

#### 👤 Người 2: POS 1 – Hoàn thiện Giỏ hàng & Mã giảm giá (`UC04`, `UC05`)
*Kế thừa file `PosScreen.tsx` đã dựng sẵn.*
* **Backend**:
  - [ ] Tạo Entity `MaGiamGia` (`maCode`, `loaiGiamGia`, `giaTriGiam`, `ngayKetThuc`).
  - [ ] Viết API kiểm tra Voucher: `POST /api/discounts/check` (trả về hợp lệ và số tiền/% giảm).
* **Frontend**:
  - [ ] Hoàn thiện tương tác giỏ hàng trên `PosScreen.tsx`: thêm/bớt số lượng, xóa món.
  - [ ] Gắn sự kiện ô nhập Voucher: Gọi API kiểm tra mã `QUATANG10` $\rightarrow$ tự động trừ tiền giảm giá.

#### 👤 Người 3: POS 2 – Dựng Transaction Đơn hàng & Thanh toán (`UC07`)
* **Backend**:
  - [ ] Tạo Entity `DonHang`, `ChiTietDonHang`.
  - [ ] Viết Service tạo đơn hàng: Lưu `DonHang` $\rightarrow$ Lưu `ChiTietDonHang` $\rightarrow$ **Trừ tồn kho `SanPham.SoLuongTon`**.
* **Frontend**:
  - [ ] Dựng sẵn Modal Thanh toán (sẽ bật lên khi bấm nút `[Thanh toán]` trên POS):
    * Giao diện chọn **Tiền mặt** (ô nhập tiền khách đưa $\rightarrow$ tự tính tiền thối).
    * Giao diện chọn **Quét mã QR** (hiển thị ảnh mã VietQR kèm số tiền).

---

### 🎯 TIÊU CHÍ HOÀN THÀNH ĐỢT 1 (DEFINITION OF DONE)
* [ ] Quản lý có thể thêm/sửa sản phẩm mới có ảnh.
* [ ] Nhân viên đăng nhập vào có popup yêu cầu Mở ca.
* [ ] Nhập SĐT khách hàng trên POS tìm được đúng tên khách.
* [ ] Nhập mã voucher trừ được tiền trên giao diện giỏ hàng.
* [ ] API tạo đơn hàng và trừ tồn kho test thành công trên Swagger.
