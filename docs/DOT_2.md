# ⚡ ĐỢT 2: CÁC TÍNH NĂNG MỞ RỘNG & LIÊN THÔNG NGHIỆP VỤ
> **Thời gian thực hiện**: Ngày 4 – Hết Ngày 5  
> **Mục tiêu**: Hoàn thiện các Use Case còn thiếu trong luồng bán hàng & khách hàng (**UC06**, **UC08**, phần lịch sử mua sắm của **UC09**) và kiểm chứng liên thông Kho – POS.

---

### PHÂN CÔNG NHIỆM VỤ ĐỢT 2

#### 👤 Tâm: Lưu nháp đơn (`UC06`)
* **Backend**:
  - [ ] Viết API Đơn nháp: `POST /api/draft-orders` (lưu tạm giỏ hàng), `GET /api/draft-orders` (lấy danh sách đơn nháp), `DELETE /api/draft-orders/{id}`.
* **Frontend**:
  - [ ] Gắn sự kiện nút **[Lưu nháp đơn]** trên `PosScreen.tsx`: gọi API lưu giỏ hiện tại và làm sạch giỏ.
  - [ ] Gắn sự kiện nút **[Đơn nháp (3)]**: mở modal danh sách đơn đang lưu tạm, bấm "Khôi phục" để nạp lại lên giỏ hàng.

#### 👤 Đạt: Trả hàng / Hoàn tiền (`UC08`)
* **Backend**:
  - [ ] Viết API Trả hàng / Hoàn tiền (`POST /api/returns` hoặc tương đương): kiểm tra điều kiện ngày đổi trả (`SoNgayChoPhepDoiTra`), đổi trạng thái đơn sang `DaTraHang` và **cộng trả lại số lượng vào tồn kho `SanPham.SoLuongTon`**.
* **Frontend**:
  - [ ] Tạo trang / modal Tra cứu hóa đơn cũ (tìm theo mã đơn / ngày) để thực hiện lập phiếu Trả hàng & Hoàn tiền.

#### 👤 Thành: Bổ sung Lịch sử mua sắm khách hàng (`UC09`)
* **Backend**:
  - [ ] Viết API `GET /api/customers/{id}/orders` trả về danh sách các đơn hàng cũ của khách hàng (Mã đơn, ngày mua, tổng tiền, trạng thái).
* **Frontend**:
  - [ ] Trên `CustomerManagementScreen.tsx`, thêm nút / modal **"Xem lịch sử mua hàng"** khi chọn một khách hàng.

#### 👤 Khang: Kiểm chứng liên thông Kho – POS
* Phối hợp với Tâm & Đạt test thực tế sau khi hoàn thành `UC11`, `UC12`:
  - [x] Tạo phiếu nhập thêm hàng ở trang Kho (`UC12`) $\rightarrow$ màn hình POS lập tức cập nhật tăng tồn kho.
  - [x] Bán hàng trên POS $\rightarrow$ quay lại trang Quản lý kho thấy tồn kho giảm đúng số lượng.

#### 👤 Hiệp : Điều phối & Review
* [ ] Ngồi cắn hạt dưa

---

### 🎯 TIÊU CHÍ HOÀN THÀNH ĐỢT 2 (DEFINITION OF DONE)
* [ ] Lưu nháp đơn hàng và khôi phục lại lên giỏ hàng POS thành công.
* [ ] Tìm lại đơn hàng cũ và thực hiện Trả hàng thành công (tồn kho được cộng trả lại).
* [ ] Xem được danh sách lịch sử mua sắm của từng khách hàng thân thiết.
* [ ] Nhập kho làm tăng tồn kho trên POS, bán hàng làm giảm tồn kho trong Quản lý kho.
