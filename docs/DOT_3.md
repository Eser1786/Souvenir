# 🏆 ĐỢT 3: BÁO CÁO THỐNG KÊ, TỔNG DUYỆT & ĐÓNG BĂNG DEMO
> **Thời gian thực hiện**: Ngày 6 – Ngày 7  
> **Mục tiêu**: Hoàn thiện tính năng Báo cáo doanh thu biểu đồ, tính năng Kiểm kê kho; Rà soát toàn bộ lỗi hệ thống; Đóng băng code và tập dượt demo bảo vệ đồ án trước giảng viên.

---

### 1. PHÂN CÔNG NHIỆM VỤ CHI TIẾT ĐỢT 3

#### 👤 Thành: Dashboard Báo cáo Doanh thu & Thống kê (`UC14`)
*Bắt đầu làm khi trong DB đã có các đơn hàng do Tâm & Đạt bán ra ở Đợt 2:*
* **Backend**:
  - Viết API `GET /api/reports/revenue`: Thống kê tổng doanh thu, số đơn theo ngày/tuần/tháng.
  - Viết API `GET /api/reports/top-products`: Lấy top 5 sản phẩm bán chạy nhất kèm số lượng bán.
  - Viết API `GET /api/reports/shifts`: Thống kê doanh thu theo từng ca làm việc.
* **Frontend**:
  - Tạo trang `ReportDashboardScreen.tsx` (Chỉ role `QuanLy` mới xem được):
    * 3 Thẻ số liệu tổng quan: **Tổng doanh thu**, **Tổng đơn hàng**, **Tổng tồn kho**.
    * Biểu đồ đường / cột doanh thu theo ngày (dùng thư viện `recharts` hoặc `chart.js`).
    * Biểu đồ tròn hoặc bảng danh sách Top 5 món bán chạy nhất.

#### 👤 Thái Anh: Chức năng Kiểm kê kho hàng (`UC13`)
* **Backend**:
  - Tạo Entity & API `PhieuKiemKe`, `ChiTietPhieuKiemKe`: Ghi nhận số lượng thực tế kiểm đếm, tính số lượng chênh lệch (Thừa / Thiếu) và cân bằng lại tồn kho.
* **Frontend**:
  - Tạo trang `StockAuditScreen.tsx`: Cho phép chọn sản phẩm, nhập số lượng đếm được trong kho, ghi lý do chênh lệch và bấm xác nhận cân đối kho.

#### 👤 Hiệp (Lead) & CẢ NHÓM: Rà soát lỗi (Bug Bashing) & Làm sạch CSDL (Ngày 6)
* [ ] Kiểm tra lỗi phân quyền: Nhân viên không thể vào được trang Báo cáo hay trang Quản lý nhân sự.
* [ ] Kiểm tra lỗi tính toán tiền: Đảm bảo Tạm tính - Giảm giá voucher - Giảm giá đổi điểm = Tổng thanh toán.
* [ ] Kiểm tra lỗi ảnh: Tất cả sản phẩm đều hiển thị ảnh đẹp, không bị vỡ link hay 404.
* [ ] Làm sạch DB và seed lại dữ liệu chuẩn đẹp: Xóa các đơn hàng test lung tung, seed lại 15 món đồ lưu niệm có ảnh đẹp, giá chuẩn, có sẵn vài ca làm việc và vài đơn hàng cũ để biểu đồ báo cáo trông đầy đặn.

---

### 2. QUY TẮC ĐÓNG BĂNG CODE (CODE FREEZE - NGÀY 7)
> [!CAUTION]
> **Sau 12:00 trưa Ngày 7: TUYỆT ĐỐI CẤM commit thêm bất kỳ tính năng mới nào.**  
> Mọi thay đổi vào phút chót đều có nguy cơ làm gãy cả hệ thống trước giờ demo.

---

### 3. KỊCH BẢN TẬP DƯỢT DEMO 6 BƯỚC CHO BUỔI BẢO VỆ
Cả nhóm ngồi lại cùng nhau, mở máy và chạy thử đúng **6 bước kịch bản chấm bài** (Dry Run ít nhất 3 lần):

* **Bước 1: Quản lý đăng nhập & kiểm kho**
  * Login bằng `admin / admin123` $\rightarrow$ Mở trang Quản lý sản phẩm xem danh sách món $\rightarrow$ Tạo phiếu nhập thêm 20 Gấu bông Teddy.
* **Bước 2: Nhân viên đăng nhập & Mở ca**
  * Đăng xuất $\rightarrow$ Login bằng nhân viên `tranthuha / password123` $\rightarrow$ Popup Mở ca hiện lên $\rightarrow$ Nhập tiền đầu ca: `1.000.000 đ` $\rightarrow$ Vào giao diện POS.
* **Bước 3: Bán hàng tại quầy POS**
  * Nhập SĐT khách hàng: `0901 234 567` (hiện tên Nguyễn Ngọc Lan, 250 điểm).
  * Bấm chọn 2 Gấu bông Teddy + 1 Ly sứ tặng Ba vào giỏ hàng.
  * Nhập mã voucher `QUATANG10` (giảm 10%).
  * Bấm nút áp dụng đổi 50 điểm lấy 5.000đ.
* **Bước 4: Thanh toán & Xuất hóa đơn**
  * Bấm nút **[Thanh toán]** $\rightarrow$ Chọn hình thức Tiền mặt hoặc Quét mã QR $\rightarrow$ Xác nhận $\rightarrow$ Popup hóa đơn thành công hiện ra $\rightarrow$ Giỏ hàng về rỗng.
* **Bước 5: Nhân viên Chốt ca**
  * Bấm nút Mở/Chốt ca trên Sidebar $\rightarrow$ Bấm Chốt ca $\rightarrow$ Xem tổng tiền mặt đã thu trong ca $\rightarrow$ Xác nhận đóng ca.
* **Bước 6: Quản lý vào xem Báo cáo doanh thu**
  * Đăng xuất $\rightarrow$ Login lại bằng `admin` $\rightarrow$ Vào trang Báo cáo doanh thu $\rightarrow$ Biểu đồ nhảy số doanh thu vừa bán ở Bước 4.

👉 *Chạy trơn tru 6 bước trên từ đầu đến cuối là chắc chắn đạt điểm tối đa!*
