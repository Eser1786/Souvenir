# BẢN PHÂN CÔNG NHIỆM VỤ & THEO DÕI TIẾN ĐỘ DỰ ÁN
## HỆ THỐNG QUẢN LÝ CỬA HÀNG ĐỒ LƯU NIỆM (MERA SOUVENIR POS)

* **Thời gian thực hiện**: Sprint 7 ngày (Cập nhật lúc: Ngày 2)
* **Quy mô nhóm**: 6 thành viên
* **Kiến trúc kỹ thuật**: .NET 10 Clean Architecture (Backend) + React 19 TypeScript Vite (Frontend) + PostgreSQL

---

## 1. BÁO CÁO HIỆN TRẠNG TOÀN BỘ 15 USE CASE (AUDIT NGÀY 2)

Nhóm đã hoàn thành xuất sắc giai đoạn **Đào móng** (Base Clean Architecture, Auth JWT) và dựng sẵn **Mặt tiền POS 3 cột chuẩn Figma**.

| Mã UC | Tên Use Case | Backend | Frontend | Trạng thái thực tế |
| :---: | :--- | :---: | :---: | :--- |
| **UC01** | **Đăng nhập** | 🟢 100% | 🟢 100% | **Xong 100%** (JWT, mã hóa BCrypt, khóa sau 5 lần sai, UI Login) |
| **UC04** | **Tạo đơn hàng (POS)** | 🔴 0% | 🟡 80% | **Đã có giao diện POS 3 cột**: Giỏ hàng, chọn món, đổi số lượng, tạm tính. *Chưa có BE lưu đơn*. |
| **UC11** | **Quản lý sản phẩm (CRUD)** | 🟡 50% | 🟡 50% | **Xong phần Đọc**: Entity `SanPham`, seed data, API `GET /api/products`, lưới ảnh. *Chưa có Thêm/Sửa/Xóa*. |
| **UC05** | **Áp dụng giảm giá** | 🔴 0% | 🟡 50% | Đã có ô nhập voucher & tính % trừ tiền trên UI POS. *Chưa có Entity/API `MaGiamGia`*. |
| **UC06** | **Lưu nháp đơn** | 🔴 0% | 🟡 40% | Đã có nút "Lưu nháp đơn" & badge "Đơn nháp (3)". *Chưa có API lưu nháp*. |
| **UC07** | **Thanh toán** | 🔴 0% | 🟡 25% | Đã có nút [Thanh toán], tính tổng tiền. *Chưa có popup Tiền mặt/QR và API trừ kho*. |
| **UC09** | **Quản lý hồ sơ khách hàng** | 🔴 0% | 🟡 20% | Đã có ô hiển thị SĐT, tên khách trên giỏ hàng POS. *Chưa có trang CRUD quản lý khách*. |
| **UC10** | **Tích điểm / Đổi điểm** | 🔴 0% | 🟡 30% | Đã có nút Đổi điểm (50 điểm = 5.000đ). *Chưa có logic lưu điểm vào DB*. |
| **UC02** | Chấm công (In/Out) | 🔴 0% | 🔴 0% | Chưa làm (đã có icon placeholder trên Sidebar) |
| **UC03** | Mở / Chốt ca làm việc | 🔴 0% | 🔴 0% | Chưa làm (đã có nhãn ca trên POS) |
| **UC08** | Trả hàng / Hoàn tiền | 🔴 0% | 🔴 0% | Chưa làm |
| **UC12** | Tạo phiếu nhập kho | 🔴 0% | 🔴 0% | Chưa làm |
| **UC13** | Kiểm kê kho | 🔴 0% | 🔴 0% | Chưa làm |
| **UC14** | Xem báo cáo (Doanh thu, Kho) | 🔴 0% | 🔴 0% | Chưa làm |
| **UC15** | Quản lý nhân viên & Phân quyền | 🔴 0% | 🔴 0% | Chưa làm (đã có bảng `NGUOI_DUNG` trong DB) |

---

## 2. MA TRẬN PHÂN CÔNG 6 NGƯỜI (MÔ HÌNH FULLSTACK VERTICAL SLICE)

Mỗi thành viên tự làm trọn vẹn cả **Backend API** và **Giao diện Frontend** cho tính năng mình phụ trách:

```
[Người 1: Tech Lead & Nhân sự] ──> UC15 (Nhân viên, Phân quyền) + Base Review
[Người 2: POS 1]               ──> UC04 (Tạo đơn) + UC05 (Giảm giá) + UC06 (Lưu nháp)
[Người 3: POS 2]               ──> UC07 (Thanh toán & Trừ kho) + UC08 (Trả hàng)
[Người 4: Kho hàng]            ──> UC11 (CRUD Sản phẩm) + UC12 (Nhập kho)
[Người 5: Vận hành]            ──> UC03 (Mở/Chốt ca) + UC02 (Chấm công) + UC13 (Kiểm kê)
[Người 6: Khách hàng & Báo cáo]──> UC09 (Khách hàng) + UC10 (Tích điểm) + UC14 (Báo cáo)
```

---

### CHI TIẾT NHIỆM VỤ TỪNG THÀNH VIÊN

### 👤 NGƯỜI 1: TECH LEAD / QUẢN TRỊ NHÂN VIÊN
* **Use Case sở hữu**: **UC15 (Quản lý nhân viên & Phân quyền)**
* **Backend Checklist**:
  - [ ] Viết API CRUD Nhân viên: `GET/POST/PUT/DELETE /api/users`.
  - [ ] Viết API Khóa / Mở khóa tài khoản nhân viên.
  - [ ] Viết API Đổi mật khẩu.
  - [ ] Gắn Authorization: Chỉ tài khoản có vai trò `QuanLy` mới được truy cập các endpoint này.
* **Frontend Checklist**:
  - [ ] Tạo trang `UsersManagementScreen.tsx` (dạng bảng danh sách nhân viên, modal thêm/sửa, nút gạt trạng thái Hoạt động / Khóa).
  - [ ] Đăng ký route `/users` vào `App.tsx` (có Role Guard chặn nếu là `NhanVien`).
* **Nhiệm vụ điều phối**: Review Pull Request, merge code vào `dev`, hỗ trợ gỡ conflict cho nhóm.

---

### 👤 NGƯỜI 2: POS 1 - BÁN HÀNG, GIỎ HÀNG & KHUYẾN MÃI
* **Use Case sở hữu**: **UC04 (Tạo đơn hàng)** + **UC05 (Giảm giá)** + **UC06 (Lưu nháp đơn)**
* **Kế thừa**: File [PosScreen.tsx](file:///e:/Hoc%20Tap/OOAD/Souvenir/frontend/src/components/Pos/PosScreen.tsx) đã có sẵn.
* **Backend Checklist**:
  - [ ] Tạo Entity `MaGiamGia` (`Id`, `MaCode`, `TenVoucher`, `LoaiGiamGia`, `GiaTriGiam`, `NgayBatDau`, `NgayKetThuc`).
  - [ ] Viết API `POST /api/discounts/check`: Kiểm tra mã voucher hợp lệ, còn hạn và trả về số tiền/% giảm giá.
  - [ ] Viết API Đơn nháp: `POST /api/draft-orders` (Lưu đơn tạm), `GET /api/draft-orders` (Lấy danh sách đơn nháp), `DELETE /api/draft-orders/{id}`.
* **Frontend Checklist**:
  - [ ] Gắn sự kiện ô nhập Voucher trên `PosScreen.tsx`: Gọi API kiểm tra mã, cập nhật tiền giảm vào giỏ.
  - [ ] Hoàn thiện nút **[Lưu nháp đơn]**: Gọi API lưu giỏ hàng hiện tại vào danh sách nháp và làm sạch giỏ.
  - [ ] Hoàn thiện nút **[Đơn nháp (3)]**: Bấm vào mở modal hiển thị các đơn đang lưu tạm; bấm "Khôi phục" thì nạp lại vào giỏ hàng.

---

### 👤 NGƯỜI 3: POS 2 - THANH TOÁN, HÓA ĐƠN & TRẢ HÀNG
* **Use Case sở hữu**: **UC07 (Thanh toán & Trừ kho)** + **UC08 (Trả hàng / Hoàn tiền)**
* **Hợp tác chặt chẽ với**: Người 2 (nhận giỏ hàng từ Người 2 để thanh toán).
* **Backend Checklist**:
  - [ ] Tạo Entity `DonHang`, `ChiTietDonHang`.
  - [ ] Viết Transaction thanh toán: Lưu `DonHang` $\rightarrow$ Lưu `ChiTietDonHang` $\rightarrow$ **Trừ `soLuongTon` của `SanPham`**.
  - [ ] Viết API Trả hàng / Hoàn tiền (`UC08`): Đổi trạng thái đơn sang `DaTraHang`, cộng trả lại số lượng vào tồn kho `SanPham`.
* **Frontend Checklist**:
  - [ ] Bắt sự kiện click nút **[Thanh toán]** trên `PosScreen.tsx` $\rightarrow$ Mở Modal Thanh toán:
    * Tùy chọn **Tiền mặt**: Nhập tiền khách đưa $\rightarrow$ tự động tính tiền thừa trả khách.
    * Tùy chọn **Chuyển khoản QR**: Hiển thị ảnh mã VietQR kèm đúng số tiền thanh toán.
  - [ ] Sau khi xác nhận thanh toán thành công: Hiển thị popup hóa đơn hoàn tất và reset giỏ hàng về rỗng.
  - [ ] Tạo trang / modal Tra cứu hóa đơn cũ để thực hiện Trả hàng.

---

### 👤 NGƯỜI 4: QUẢN LÝ KHO & SẢN PHẨM
* **Use Case sở hữu**: **UC11 (Hoàn thiện CRUD Sản phẩm)** + **UC12 (Phiếu nhập kho)**
* **Kế thừa**: Entity `SanPham`, `DanhMucSanPham` đã có trong `Souvenir.Domain`.
* **Backend Checklist**:
  - [ ] Bổ sung các API còn thiếu của sản phẩm:
    * `POST /api/products`: Thêm sản phẩm mới (trường ảnh trong DB là `imageUrl`, C# là `HinhAnh`).
    * `PUT /api/products/{id}`: Sửa thông tin / giá bán / trạng thái.
    * `DELETE /api/products/{id}`: Xóa hoặc ngưng kinh doanh.
  - [ ] Tạo Entity `PhieuNhapKho`, `ChiTietPhieuNhapKho`.
  - [ ] Viết API Tạo phiếu nhập kho: Ghi nhận phiếu và **cộng số lượng vào tồn kho `SanPham.SoLuongTon`**.
* **Frontend Checklist**:
  - [ ] Tạo trang `ProductManagementScreen.tsx`: Bảng danh sách sản phẩm, lọc theo danh mục, form modal Thêm/Sửa (có ô paste URL ảnh và preview trực tiếp).
  - [ ] Tạo trang `GoodsReceiptScreen.tsx`: Form tạo phiếu nhập kho từ Nhà cung cấp.

---

### 👤 NGƯỜI 5: VẬN HÀNH - CA LÀM VIỆC, CHẤM CÔNG & KIỂM KÊ
* **Use Case sở hữu**: **UC03 (Mở / Chốt ca)** + **UC02 (Chấm công)** + **UC13 (Kiểm kê kho)**
* **Backend Checklist**:
  - [ ] Tạo Entity `CaLamViec`:
    * API Mở ca: Nhập số tiền ban đầu trong két tiền mặt.
    * API Chốt ca: Tính tổng doanh thu tiền mặt trong ca, so sánh với số tiền thực tế nhân viên kiểm đếm để tính tiền chênh lệch.
  - [ ] Tạo Entity `ChamCong`: API Check-in (Vào ca), Check-out (Tan ca).
  - [ ] Tạo Entity `PhieuKiemKe`, `ChiTietPhieuKiemKe`: API lưu số lượng kiểm kê thực tế và cập nhật lại tồn kho.
* **Frontend Checklist**:
  - [ ] Tạo Popup **Mở ca**: Hiển thị khi nhân viên vào POS mà chưa có ca hoạt động.
  - [ ] Tạo Popup **Chốt ca**: Cho phép đếm tiền mặt kết ca và xem tổng kết doanh thu ca.
  - [ ] Tạo modal / nút Chấm công nhanh trên Sidebar.
  - [ ] Tạo trang Lập phiếu kiểm kê kho.

---

### 👤 NGƯỜI 6: KHÁCH HÀNG & BÁO CÁO THỐNG KÊ
* **Use Case sở hữu**: **UC09 (Hồ sơ Khách hàng)** + **UC10 (Tích / Đổi điểm)** + **UC14 (Báo cáo Thống kê)**
* **Backend Checklist**:
  - [ ] Tạo Entity `KhachHang`: Viết CRUD Khách hàng, API tìm kiếm khách nhanh bằng số điện thoại (`GET /api/customers?phone=...`).
  - [ ] Xử lý logic điểm thưởng:
    * Khi tạo đơn thành công (kết hợp với Người 3): Cộng `1%` giá trị đơn vào `DiemTichLuy`.
    * Khi đổi điểm: Trừ điểm tích lũy của khách tương ứng với số tiền giảm giá.
  - [ ] Viết API Báo cáo (`UC14`): Thống kê doanh thu theo ngày/tháng, top 5 món bán chạy nhất, doanh thu theo ca làm việc.
* **Frontend Checklist**:
  - [ ] Tạo trang `CustomerManagementScreen.tsx`: Bảng danh sách khách hàng và lịch sử điểm thưởng.
  - [ ] Kết nối ô tìm SĐT khách hàng trên `PosScreen.tsx` với API tìm kiếm thật.
  - [ ] Tạo trang `ReportDashboardScreen.tsx`: Vẽ biểu đồ doanh thu bằng thư viện `recharts` hoặc `chart.js`, bảng tổng kết doanh số (chỉ role `QuanLy` truy cập).

---

## 3. THỨ TỰ TRIỂN KHAI: AI LÀM TRƯỚC, AI LÀM SAU? (3 ĐỢT SÓNG)

Nhờ đã có sẵn Auth + Khung POS + API Sản phẩm, **cả 6 người đều bắt tay vào làm ngay được mà không ai phải ngồi chờ**. Tuy nhiên, thứ tự các tính năng sẽ chia theo 3 đợt:

### 🌊 ĐỢT 1: LÀM NGAY BÂY GIỜ (Chiều Ngày 2 – Ngày 3)
*Tất cả 6 người làm song song các phần độc lập, tạo nền móng nghiệp vụ:*
* **Người 4**: Hoàn thiện CRUD Sản phẩm (Thêm/Sửa/Xóa) + Phiếu nhập kho (`UC11`, `UC12`). *(Có sẵn Entity Sản phẩm, làm được ngay)*.
* **Người 5**: Làm chức năng **Mở ca làm việc** (`UC03`) + Chấm công (`UC02`). *(Làm popup nhập tiền đầu ca để đơn hàng có `ShiftId`)*.
* **Người 6**: Quản lý Khách hàng (`UC09`). *(Làm bảng khách hàng và API tìm kiếm SĐT để Người 2 nhúng vào POS)*.
* **Người 1**: Quản lý Nhân viên (`UC15`). *(Trang danh sách nhân viên, phân quyền role, khóa tài khoản)*.
* **Người 2**: Hoàn thiện Giỏ hàng POS (`UC04`) + API & logic Mã giảm giá (`UC05`). *(Kế thừa `PosScreen.tsx` có sẵn)*.
* **Người 3**: Viết Entity `DonHang`, API Tạo đơn & Transaction Thanh toán trừ tồn kho (`UC07`).

---

### 🌊 ĐỢT 2: GHÉP NỐI & TÍNH NĂNG PHỤ THUỘC (Ngày 4 – Ngày 5)
*Các tính năng này **BẮT BUỘC PHẢI ĐỢI** Đợt 1 hoàn thành mới làm được:*
1. **Người 2 & Người 3 ghép nối (POS Hoàn chỉnh)**: 
   - Giỏ hàng của Người 2 $\rightarrow$ Bấm nút [Thanh toán] $\rightarrow$ Mở Modal chọn Tiền mặt/QR của Người 3 $\rightarrow$ Ghi nhận đơn hàng & Trừ tồn kho thật trong DB.
2. **Người 6 ghép Tích điểm (`UC10`)**: 
   - **Phải đợi Người 3** viết xong API thanh toán đơn hàng thì Người 6 mới gắn được logic: *Đơn hoàn tất $\rightarrow$ tự động cộng 1% điểm cho khách hàng*.
3. **Người 3 làm Trả hàng / Hoàn tiền (`UC08`)**: 
   - **Phải đợi có đơn hàng đã thanh toán** thành công thì mới làm được giao diện tra cứu đơn cũ để bấm nút "Trả hàng / Hoàn tiền" và cộng lại kho.
4. **Người 2 làm Lưu nháp đơn (`UC06`)**: 
   - Sau khi giỏ hàng chạy mượt, gắn thêm nút lưu đơn tạm vào DB và modal xem lại danh sách đơn nháp.

---

### 🌊 ĐỢT 3: VỀ ĐÍCH & BÁO CÁO (Ngày 6)
*Làm sau cùng vì cần dữ liệu thực tế phát sinh của toàn bộ hệ thống:*
1. **Người 6 làm BÁO CÁO THỐNG KÊ (`UC14`) - Làm cuối cùng**:
   - *Lý do*: Biểu đồ doanh thu chỉ vẽ được khi trong CSDL đã có **dữ liệu đơn hàng thật** (do Người 2 & 3 bán ra ở Ngày 3-5) và **dữ liệu ca làm việc** (Người 5).
2. **Người 5 làm KIỂM KÊ KHO (`UC13`)**:
   - Làm sau khi đã có giao dịch Nhập kho (Người 4) và Bán hàng xuất kho (Người 3) để kiểm tra số lượng lệch thực tế.
3. **Cả nhóm + Lead (Người 1)**:
   - Rà soát bug, kiểm tra luồng thông suốt và đóng băng code (Code Freeze) vào Ngày 7.

---

## 4. LỘ TRÌNH 7 NGÀY (TIẾN TRÌNH THỰC THI)

* **Ngày 1 (Đã hoàn thành)**: Xây móng Clean Architecture, JWT Auth, Database DDL/Seed, dựng xong giao diện POS 3 cột chuẩn Figma.
* **Ngày 2 (Hôm nay)**:
  - Cả 6 người pull code từ `origin/dev`.
  - Người 1 làm Quản lý nhân viên; Người 4 hoàn thiện CRUD Sản phẩm; Người 5 làm Ca làm việc; Người 6 làm Khách hàng.
  - Người 2 & 3 dựng API Đơn hàng & Thanh toán.
* **Ngày 3 & 4 (Đột phá Core POS)**:
  - Người 2 & 3 ghép nối thông luồng: *Chọn món $\rightarrow$ Áp mã $\rightarrow$ Bấm Thanh toán $\rightarrow$ DB tạo Order & Trừ tồn kho*.
  - Người 4 hoàn thành Nhập kho (nhập hàng thì tồn ở POS tự tăng).
  - Người 5 hoàn thành Chốt ca; Người 6 hoàn thành Tích điểm.
* **Ngày 5 (Ghép nối toàn bộ hệ thống)**:
  - Ghép các tính năng vệ tinh: Trả hàng (`UC08`), Lưu nháp (`UC06`), Kiểm kê (`UC13`).
  - Chạy thử 1 vòng toàn bộ kịch bản thực tế.
* **Ngày 6 (Làm Báo cáo & Đập Bug)**:
  - Người 6 lấy dữ liệu các đơn hàng đã bán để vẽ biểu đồ Báo cáo (`UC14`).
  - Cả nhóm rà soát và fix các lỗi vỡ layout, sai tiền, âm tồn kho.
* **Ngày 7 (ĐÓNG BĂNG CODE - CODE FREEZE)**:
  - **Sau 12h trưa Ngày 7: Cấm commit thêm bất kỳ tính năng mới nào.**
  - Seed lại dữ liệu sạch đẹp vào Database.
  - Cả nhóm tập dượt demo kịch bản bảo vệ ít nhất 3 lần.

---

## 5. QUY ƯỚC LÀM VIỆC ĐỂ TRÁNH XUNG ĐỘT GIT

1. **Lệnh bắt đầu làm việc mỗi ngày**:
   ```bash
   git checkout -b feat/<ten-tinh-nang> origin/dev
   # Vi du: git checkout -b feat/user-management origin/dev
   ```
2. **Quy tắc phân chia file Frontend**:
   Mỗi người tạo thư mục riêng trong `frontend/src/components/`, không sửa đè file của người khác:
   * Người 1: `components/Users/`
   * Người 2 & 3: Hoàn thiện trong `components/Pos/`
   * Người 4: `components/Inventory/` và `components/Products/`
   * Người 5: `components/Shifts/`
   * Người 6: `components/Customers/` và `components/Reports/`
3. **Tài khoản test đăng nhập có sẵn**:
   * **Quản lý (Admin)**: `admin` / `admin123`
   * **Nhân viên (Staff)**: `tranthuha` / `password123`
