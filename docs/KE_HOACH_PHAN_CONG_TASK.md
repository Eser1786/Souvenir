# BẢN PHÂN CÔNG NHIỆM VỤ & THEO DÕI TIẾN ĐỘ DỰ ÁN
## HỆ THỐNG QUẢN LÝ CỬA HÀNG ĐỒ LƯU NIỆM (MERA SOUVENIR POS)

* **Thời gian thực hiện**: Sprint 7 ngày (Cập nhật lúc: Trưa Ngày 3 - Đợt 1 gần hoàn tất)
* **Quy mô nhóm**: 6 thành viên
* **Kiến trúc kỹ thuật**: .NET 10 Clean Architecture (Backend) + React 19 TypeScript Vite (Frontend) + PostgreSQL

---

## 1. BÁO CÁO HIỆN TRẠNG TOÀN BỘ 15 USE CASE (AUDIT TRƯA NGÀY 3)

Nhóm đã bứt tốc thần tốc, **hoàn thành 5/6 phân hệ của Đợt 1** và thậm chí đã giải quyết sớm một số khâu liên thông của Đợt 2 (Thanh toán POS, Chốt ca, Tích điểm).

> [!IMPORTANT]
> **TIẾN ĐỘ ĐỢT 1 HIỆN TẠI**:
> * **Đã xong 5/6 phân hệ**: Đăng nhập (`UC01`), Nhân sự (`UC15` - **Hiệp**), Khách hàng (`UC09` - **Thành**), Ca & Chấm công (`UC02`, `UC03` - **Thái Anh**), Bán hàng POS & Giảm giá (`UC04`, `UC05` - **Tâm**), Thanh toán & Tích điểm (`UC07`, `UC10` - **Đạt**).
> * **Còn đúng 1 phần duy nhất của Đợt 1 đang hoàn thiện**: **Phân hệ Kho & Sản phẩm của Khang** gồm **UC11** (hoàn thiện CRUD Thêm/Sửa/Xóa sản phẩm) và **UC12** (Lập phiếu nhập kho).

| Mã UC | Tên Use Case | Phụ trách | Backend | Frontend | Đánh giá trạng thái thực tế |
| :---: | :--- | :---: | :---: | :---: | :--- |
| **UC01** | **Đăng nhập** | **Hiệp** | 🟢 100% | 🟢 100% | **Xong 100%** (JWT, BCrypt, khóa sau 5 lần sai, UI Login) |
| **UC02** | **Chấm công (In/Out)** | **Thái Anh** | 🟢 100% | 🟢 100% | **Xong 100%** (API check-in/out, AttendanceModal trên Sidebar) |
| **UC03** | **Mở / Chốt ca làm việc** | **Thái Anh** | 🟢 100% | 🟢 100% | **Xong 100%** (API open/close/explain ca, OpenShift & CloseShift Modal) |
| **UC04** | **Tạo đơn hàng (POS)** | **Tâm** | 🟢 100% | 🟢 100% | **Xong 100%** (Chọn món, giỏ hàng, tính tổng, gọi API Orders) |
| **UC05** | **Áp dụng giảm giá** | **Tâm** | 🟢 100% | 🟢 100% | **Xong 100%** (Entity `MaGiamGia`, API `POST /discounts/check`, trừ tiền tự động) |
| **UC07** | **Thanh toán & Trừ kho** | **Đạt** | 🟢 100% | 🟢 100% | **Xong 100%** (Transaction lưu Đơn hàng $\rightarrow$ trừ `soLuongTon`, modal Tiền mặt/VietQR) |
| **UC09** | **Quản lý hồ sơ khách hàng** | **Thành** | 🟡 80% | 🟡 80% | **Gần xong**: Đã có CRUD Khách hàng, search SĐT; *còn thiếu xem Lịch sử mua sắm*. |
| **UC10** | **Tích điểm / Đổi điểm** | **Thành / Đạt** | 🟢 100% | 🟢 100% | **Xong 100%** (Cộng 1% điểm khi thanh toán, đổi điểm trừ tiền trên POS) |
| **UC15** | **Quản lý nhân viên & Phân quyền** | **Hiệp** | 🟢 100% | 🟢 100% | **Xong 100%** (UsersController CRUD, khóa tài khoản, UsersManagementScreen) |
| **UC11** | **Quản lý sản phẩm (CRUD)** | **Khang** | 🟡 50% | 🟡 50% | ⏳ **PHẦN CÒN LẠI CỦA ĐỢT 1**: Đã có `GET /products` hiển thị POS; cần bổ sung API & Modal Thêm/Sửa/Xóa. |
| **UC12** | **Tạo phiếu nhập kho** | **Khang** | 🔴 0% | 🔴 0% | ⏳ **PHẦN CÒN LẠI CỦA ĐỢT 1**: Cần làm Entity `PhieuNhapKho` & màn hình Lập phiếu nhập kho. |
| **UC06** | **Lưu nháp đơn** | **Tâm** | 🔴 0% | 🟡 40% | Kế hoạch Đợt 2 (Đã có UI nút Lưu nháp, cần gắn API lưu tạm giỏ) |
| **UC08** | **Trả hàng / Hoàn tiền** | **Đạt** | 🔴 0% | 🔴 0% | Kế hoạch Đợt 2 (Cần làm màn hình tra cứu đơn cũ để trả hàng) |
| **UC13** | **Kiểm kê kho** | **Thái Anh** | 🔴 0% | 🔴 0% | Kế hoạch Đợt 3 (Cân bằng tồn kho thực tế) |
| **UC14** | **Xem báo cáo (Doanh thu, Kho)** | **Thành** | 🔴 0% | 🔴 0% | Kế hoạch Đợt 3 (Biểu đồ doanh thu Recharts/Chart.js) |

---

## 2. MA TRẬN PHÂN CÔNG 6 THÀNH VIÊN (MÔ HÌNH FULLSTACK VERTICAL SLICE)

Mỗi thành viên tự làm trọn vẹn cả **Backend API** và **Giao diện Frontend** cho tính năng mình phụ trách:

```
[Hiệp: Tech Lead & Nhân sự]     ──> UC15 (Nhân viên, Phân quyền) + Base Architecture & Review
[Tâm: POS 1]                    ──> UC04 (Tạo đơn) + UC05 (Giảm giá) + UC06 (Lưu nháp)
[Đạt: POS 2]                    ──> UC07 (Thanh toán & Trừ kho) + UC08 (Trả hàng)
[Khang: Kho hàng]               ──> UC11 (CRUD Sản phẩm) + UC12 (Nhập kho)
[Thái Anh: Vận hành]            ──> UC03 (Mở/Chốt ca) + UC02 (Chấm công) + UC13 (Kiểm kê)
[Thành: Khách hàng & Báo cáo]   ──> UC09 (Khách hàng) + UC10 (Tích điểm) + UC14 (Báo cáo)
```

---

### CÁC CÔNG VIỆC CÒN LẠI CỦA TỪNG THÀNH VIÊN

### 👤 HIỆP: TECH LEAD / ĐIỀU PHỐI & BẢO MẬT
* **Nhiệm vụ còn lại**:
  - [ ] Review Pull Request, merge code vào `dev`, hỗ trợ gỡ conflict cho nhóm.
  - [ ] Điều phối kiểm thử toàn luồng E2E và quản lý Code Freeze (Ngày 7).

---

### 👤 TÂM: POS 1 - LƯU NHÁP ĐƠN HÀNG (`UC06`)
* **Backend Checklist**:
  - [ ] Viết API Đơn nháp: `POST /api/draft-orders` (Lưu đơn tạm), `GET /api/draft-orders` (Lấy danh sách đơn nháp), `DELETE /api/draft-orders/{id}` *(Đợt 2)*.
* **Frontend Checklist**:
  - [ ] Hoàn thiện nút **[Lưu nháp đơn]**: Gọi API lưu giỏ hàng hiện tại vào danh sách nháp và làm sạch giỏ *(Đợt 2)*.
  - [ ] Hoàn thiện nút **[Đơn nháp (3)]**: Bấm vào mở modal hiển thị các đơn đang lưu tạm; bấm "Khôi phục" thì nạp lại vào giỏ hàng *(Đợt 2)*.

---

### 👤 ĐẠT: POS 2 - TRẢ HÀNG & HOÀN TIỀN (`UC08`)
* **Backend Checklist**:
  - [ ] Viết API Trả hàng / Hoàn tiền (`UC08`): Kiểm tra hạn trả hàng, đổi trạng thái đơn sang `DaTraHang`, cộng trả lại số lượng vào tồn kho `SanPham` *(Đợt 2)*.
* **Frontend Checklist**:
  - [ ] Tạo trang / modal Tra cứu hóa đơn cũ để thực hiện Trả hàng *(Đợt 2)*.

---

### 👤 KHANG: QUẢN LÝ KHO & SẢN PHẨM (`UC11`, `UC12` - PHẦN CÒN LẠI ĐỢT 1)
* **Kế thừa**: Entity `SanPham`, `DanhMucSanPham` và API `GET /api/products` đã có sẵn.
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

### 👤 THÁI ANH: KIỂM KÊ KHO HÀNG (`UC13`)
* **Backend Checklist**:
  - [ ] Tạo Entity `PhieuKiemKe`, `ChiTietPhieuKiemKe`: API lưu số lượng kiểm kê thực tế và cập nhật lại tồn kho *(Đợt 3)*.
* **Frontend Checklist**:
  - [ ] Tạo trang `StockAuditScreen.tsx`: Cho phép chọn sản phẩm, nhập số lượng đếm được trong kho, ghi lý do chênh lệch và xác nhận cân đối kho *(Đợt 3)*.

---

### 👤 THÀNH: LỊCH SỬ MUA HÀNG (`UC09`) & BÁO CÁO THỐNG KÊ (`UC14`)
* **Backend Checklist**:
  - [ ] Viết API `GET /api/customers/{id}/orders` trả về danh sách các đơn hàng cũ của khách hàng *(Đợt 2)*.
  - [ ] Viết API Báo cáo (`UC14`): Thống kê doanh thu theo ngày/tháng, top 5 món bán chạy nhất, doanh thu theo ca làm việc *(Đợt 3)*.
* **Frontend Checklist**:
  - [ ] Bổ sung nút / modal xem Lịch sử mua hàng của khách trong `CustomerManagementScreen.tsx` *(Đợt 2)*.
  - [ ] Tạo trang `ReportDashboardScreen.tsx`: Vẽ biểu đồ doanh thu bằng thư viện `recharts` hoặc `chart.js`, bảng tổng kết doanh số (chỉ role `QuanLy` truy cập) *(Đợt 3)*.

---

## 3. THỨ TỰ TRIỂN KHAI CÁC CÔNG VIỆC CÒN LẠI (3 ĐỢT)

### 🌊 ĐỢT 1: CHỐT NỐT PHẦN CÒN LẠI (Chiều Ngày 3)
* **Khang**: Hoàn thiện CRUD Sản phẩm (Thêm/Sửa/Xóa) + Phiếu nhập kho (`UC11`, `UC12`).

### 🌊 ĐỢT 2: MỞ RỘNG & LIÊN THÔNG (Ngày 4 – Ngày 5)
1. **Tâm**: Hoàn thiện **Lưu nháp đơn** (`UC06`).
2. **Đạt**: Hoàn thiện **Trả hàng / Hoàn tiền** (`UC08`).
3. **Thành**: Bổ sung xem **Lịch sử mua sắm khách hàng** (`UC09`).
4. **Khang**: Phối hợp với Tâm & Đạt kiểm chứng liên thông Kho – POS (nhập kho tăng tồn, bán hàng giảm tồn).

### 🌊 ĐỢT 3: VỀ ĐÍCH & BÁO CÁO (Ngày 6 – Ngày 7)
1. **Thành**: Làm Dashboard **Báo cáo thống kê (`UC14`)** khi đã có dữ liệu đơn hàng thực tế.
2. **Thái Anh**: Làm **Kiểm kê kho (`UC13`)** đối soát tồn kho thực tế.
3. **Hiệp (Lead) & Cả nhóm**: Rà soát bug, làm sạch dữ liệu DB, đóng băng code (Code Freeze) và tập dượt demo bảo vệ.

---

## 4. LỘ TRÌNH 7 NGÀY (TIẾN TRÌNH THỰC THI)

* **Ngày 1 (Đã xong)**: Xây móng Clean Architecture, JWT Auth, Database DDL/Seed, dựng giao diện POS 3 cột chuẩn Figma.
* **Ngày 2 & 3 (Đợt 1 - Hiện tại)**:
  - Hiệp xong Quản lý nhân viên; Tâm xong Giỏ hàng & Voucher; Đạt xong Thanh toán & Trừ kho; Thái Anh xong Ca & Chấm công; Thành xong Khách hàng.
  - **Khang đang hoàn tất nốt CRUD Sản phẩm & Nhập kho.**
* **Ngày 4 & 5 (Đợt 2 - Tăng tốc ghép nối)**:
  - Tâm hoàn thiện Lưu nháp đơn (`UC06`).
  - Đạt hoàn thiện Trả hàng / Hoàn tiền (`UC08`).
  - Thành hoàn thiện xem Lịch sử đơn hàng của khách.
  - Khang hoàn thành Nhập kho liên thông POS.
* **Ngày 6 (Đợt 3 - Báo cáo & Đập Bug)**:
  - Thành vẽ biểu đồ Báo cáo doanh thu (`UC14`).
  - Thái Anh làm Kiểm kê kho (`UC13`).
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
   # Vi du: git checkout -b feat/inventory-khang origin/dev
   ```
2. **Quy tắc phân chia thư mục Frontend**:
   Mỗi người quản lý thư mục riêng trong `frontend/src/components/`, không sửa đè file của nhau:
   * **Hiệp**: `components/UsersManagement/`
   * **Tâm**: `components/Pos/`
   * **Đạt**: `components/Payment/`
   * **Khang**: `components/Products/` và `components/Inventory/`
   * **Thái Anh**: `components/Shifts/`
   * **Thành**: `components/Customer/` và `components/Reports/`
3. **Tài khoản test đăng nhập có sẵn**:
   * **Quản lý (Admin)**: `admin` / `admin123`
   * **Nhân viên (Staff)**: `tranthuha` / `password123`
3. **Tài khoản test đăng nhập có sẵn**:
   * **Quản lý (Admin)**: `admin` / `admin123`
   * **Nhân viên (Staff)**: `tranthuha` / `password123`
