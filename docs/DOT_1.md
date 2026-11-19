# 🚀 ĐỢT 1: HOÀN THIỆN PHẦN CÒN LẠI (KHO & SẢN PHẨM)
> **Thời gian thực hiện**: Chiều Ngày 3  
> **Trạng thái**: 5/6 thành viên (**Hiệp, Tâm, Đạt, Thái Anh, Thành**) đã hoàn thành 100% nhiệm vụ Đợt 1. Hiện chỉ còn phần của **Khang** để khép kín Đợt 1.

---

### PHÂN CÔNG NHIỆM VỤ CÒN LẠI CỦA ĐỢT 1

#### 👤 Khang: CRUD Sản phẩm & Phiếu nhập kho (`UC11`, `UC12`)
*Đã có sẵn Entity `SanPham` và API `GET /api/products` (cột ảnh trong DB là `imageUrl`, Entity C# là `HinhAnh`).*
* **Backend**:
  - [x] Viết API Thêm sản phẩm mới: `POST /api/products` (nhận `imageUrl` / map vào `HinhAnh`).
  - [x] Viết API Cập nhật sản phẩm: `PUT /api/products/{id}`.
  - [x] Viết API Xóa / Ngừng kinh doanh sản phẩm: `DELETE /api/products/{id}`.
  - [x] Tạo Entity & API `PhieuNhapKho`, `ChiTietPhieuNhapKho` (khi nhập kho thì **cộng thêm vào tồn kho `soLuongTon`**).
* **Frontend**:
  - [x] Tạo trang `ProductManagementScreen.tsx` (trong `components/Products/`): Bảng danh sách sản phẩm, lọc theo danh mục, modal Thêm/Sửa (có ô paste URL ảnh và preview trực tiếp).
  - [x] Tạo trang `GoodsReceiptScreen.tsx` (trong `components/Inventory/`): Form tạo phiếu nhập hàng từ Nhà cung cấp.

---

### 🎯 TIÊU CHÍ KHÉP LẠI ĐỢT 1 (DEFINITION OF DONE)
* [x] Quản lý có thể thêm, sửa, xóa sản phẩm mới có ảnh (`imageUrl`).
* [x] Tạo phiếu nhập kho thành công và số lượng tồn kho của sản phẩm tăng lên tương ứng.
