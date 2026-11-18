# ⚡ ĐỢT 2: GHÉP NỐI TOÀN DIỆN & THÔNG LUỒNG BÁN HÀNG POS
> **Thời gian thực hiện**: Ngày 4 – Hết Ngày 5  
> **Mục tiêu**: Ghép nối các phân hệ đã làm ở Đợt 1 thành một chuỗi vận hành khép kín: *Mở ca $\rightarrow$ Chọn món $\rightarrow$ Áp mã $\rightarrow$ Tích điểm $\rightarrow$ Thanh toán trừ kho $\rightarrow$ Xuất hóa đơn $\rightarrow$ Trả hàng / Lưu nháp $\rightarrow$ Chốt ca*.

---

### 1. PHÂN CÔNG NHIỆM VỤ CHI TIẾT ĐỢT 2

#### 👥 CẶP TRỌNG ĐIỂM: Người 2 + Người 3 (Thông luồng POS hoàn chỉnh)
*Đây là bước quan trọng nhất của toàn bộ đồ án, 2 bạn ngồi cùng nhau hoặc voice chat để ghép:*
* **Người 2**: 
  - Đóng gói toàn bộ `cartItems`, `appliedVoucher`, `customerId`, `shiftId` từ giỏ hàng POS khi bấm nút **[Thanh toán]** và truyền sang cho Modal của Người 3.
  - Làm tính năng **Lưu nháp đơn** (`UC06`): Nút "Lưu nháp đơn" gọi API lưu đơn tạm; Nút "Đơn nháp (3)" mở modal xem danh sách và nạp lại đơn nháp vào giỏ.
* **Người 3**:
  - Nhận dữ liệu giỏ hàng, mở Modal thanh toán (Tiền mặt / Quét mã QR).
  - Khi thu ngân bấm "Xác nhận thanh toán": Gọi API tạo đơn hàng $\rightarrow$ Lưu DB $\rightarrow$ **Trừ tồn kho `SanPham.SoLuongTon`**.
  - Hiển thị Popup hóa đơn in thành công và reset giỏ hàng về trống.
  - Làm tính năng **Trả hàng / Hoàn tiền** (`UC08`): Tạo trang tra cứu hóa đơn cũ bằng mã đơn $\rightarrow$ Bấm nút "Trả hàng" $\rightarrow$ Đổi trạng thái đơn thành `DaTraHang` và **cộng trả lại số lượng vào tồn kho**.

#### 👤 Người 6: Gắn logic Tích điểm / Đổi điểm vào POS (`UC10`)
*Sau khi Người 3 hoàn thành API tạo đơn hàng, Người 6 cắm logic điểm vào:*
* **Backend**:
  - Gắn sự kiện sau khi thanh toán đơn hàng thành công: Tự động cộng **1% giá trị đơn hàng** vào `DiemTichLuy` của khách hàng.
  - Xử lý khi khách chọn "Đổi điểm": 50 điểm = 5.000đ (trừ điểm tích lũy của khách và trừ tiền tương ứng trên hóa đơn).
* **Frontend**:
  - Hoàn thiện nút gạt "Đổi điểm: 50 điểm = 5.000 đ · Áp dụng" trên `PosScreen.tsx`.

#### 👤 Người 5: Chức năng Chốt ca kiểm tiền cuối ngày (`UC03`)
* **Backend**:
  - Viết API Chốt ca `POST /api/shifts/close`:
    * Tính tổng tiền mặt thu được từ các đơn hàng bán trong ca đó.
    * Ghi nhận số tiền thực tế nhân viên kiểm đếm trong két.
    * Tự động tính chênh lệch: `TienThucTe - (TienDauCa + TienDoanhThu)`.
* **Frontend**:
  - Tạo Popup **Chốt ca**: Hiển thị bảng tổng kết doanh thu ca làm việc, ô nhập tiền thực tế kiểm đếm, ghi chú chênh lệch và nút xác nhận Đóng ca.

#### 👤 Người 4: Kiểm chứng liên thông giữa Kho và POS
* Phối hợp với Người 2 & 3 để test thực tế:
  - Tạo phiếu nhập thêm 50 gấu bông ở trang Kho (`UC12`) $\rightarrow$ Màn hình POS lập tức thấy tồn kho tăng lên.
  - Màn hình POS bán đi 5 gấu bông $\rightarrow$ Quay lại trang Quản lý kho thấy tồn kho bị giảm đi 5.

---

### 🎯 TIÊU CHÍ HOÀN THÀNH ĐỢT 2 (DEFINITION OF DONE)
* [ ] Thu ngân bấm [Thanh toán] $\rightarrow$ Chọn tiền mặt hoặc QR $\rightarrow$ Bấm xác nhận $\rightarrow$ Tạo đơn thành công và số tồn kho của sản phẩm bị trừ ngay lập tức.
* [ ] Khách hàng sau khi mua đơn được tự động cộng điểm thưởng.
* [x] Có thể lưu nháp đơn hàng và khôi phục lại giỏ hàng. (Đã hoàn thành bởi Tâm - UC06)
* [ ] Tìm lại đơn hàng cũ và bấm Trả hàng thành công (tồn kho được cộng lại).
* [ ] Thu ngân bấm Chốt ca, hệ thống tính chuẩn doanh thu tiền mặt trong ca.
