# TÀI LIỆU ĐẶC TẢ USE CASE (USE CASE SPECIFICATION)
## HỆ THỐNG QUẢN LÝ CỬA HÀNG BÁN ĐỒ LƯU NIỆM (SOUVENIR POS)

---

## 1. TỔNG QUAN HỆ THỐNG VÀ PHẠM VI USE CASE

### 1.1 Danh sách Actor
| STT | Tên Actor | Loại Actor | Ý nghĩa / Ghi chú |
|:---:|---|---|---|
| 1 | **Quản lý** | Human Actor | Quản trị chính của hệ thống, toàn quyền vận hành, kho hàng, nhân sự và báo cáo. |
| 2 | **Nhân viên** | Human Actor | Thu ngân / nhân viên bán hàng, thực hiện bán hàng, mở/chốt ca và chấm công. |
| 3 | **Cổng thanh toán** | System Actor | Hệ thống bên ngoài xử lý các giao dịch thanh toán trực tuyến (Quét mã QR, Chuyển khoản ngân hàng). |

### 1.2 Danh sách Use Case
| STT | Mã UC | Tên Use Case | Phân hệ | Actor chính | Ghi chú quan hệ |
|:---:|:---:|---|---|---|---|
| 1 | **UC01** | Đăng nhập | Ca làm việc & Chấm công | Nhân viên, Quản lý | — |
| 2 | **UC02** | Chấm công (In/Out) | Ca làm việc & Chấm công | Nhân viên, Quản lý | — |
| 3 | **UC03** | Mở/chốt ca làm việc | Ca làm việc & Chấm công | Nhân viên, Quản lý | Extend: Báo cáo ca |
| 4 | **UC04** | Tạo/Chỉnh sửa đơn hàng | Bán hàng & Thanh toán | Nhân viên | Include: UC07; Extend: UC05, UC06 |
| 5 | **UC05** | Áp dụng giảm giá | Bán hàng & Thanh toán | Nhân viên | Extend to UC04 |
| 6 | **UC06** | Lưu nháp đơn | Bán hàng & Thanh toán | Nhân viên | Extend to UC04 |
| 7 | **UC07** | Thanh toán | Bán hàng & Thanh toán | Nhân viên, Cổng thanh toán | Included in UC04; Extend: UC10 |
| 8 | **UC08** | Trả hàng / Hoàn tiền | Bán hàng & Thanh toán | Quản lý, Cổng thanh toán | Extend to UC07 |
| 9 | **UC09** | Quản lý hồ sơ khách hàng | Khách hàng | Nhân viên, Quản lý | Include in UC10 |
| 10 | **UC10** | Tích điểm / Đổi điểm | Khách hàng | Nhân viên | Extend to UC07; Include: UC09 |
| 11 | **UC11** | Quản lý sản phẩm (CRUD) | Kho hàng | Quản lý, Nhân viên | Extend from UC12 |
| 12 | **UC12** | Tạo phiếu nhập kho | Kho hàng | Quản lý | Extend: UC11 |
| 13 | **UC13** | Kiểm kê kho | Kho hàng | Quản lý | — |
| 14 | **UC14** | Xem báo cáo (Doanh thu, Kho, Ca) | Báo cáo & Hệ thống | Quản lý | — |
| 15 | **UC15** | Quản lý nhân viên & Phân quyền | Báo cáo & Hệ thống | Quản lý | — |

---

## 2. CHI TIẾT ĐẶC TẢ TỪNG USE CASE

---

### UC01: ĐẶC TẢ USE CASE ĐĂNG NHẬP

#### 1. Thông tin chung
- **Mã Use Case**: UC01
- **Tên Use Case**: Đăng nhập
- **Actor chính**: Nhân viên, Quản lý (Quản lý kế thừa vai trò của Nhân viên)
- **Tóm tắt**: Nhân viên và Quản lý đăng nhập vào hệ thống bằng tài khoản được cấp để xác thực danh tính và phân quyền truy cập theo vai trò.

#### 2. Tiền điều kiện (Pre-conditions)
- Người dùng đã được Quản lý cấp tài khoản trong hệ thống và tài khoản đang ở trạng thái hoạt động (`HOAT_DONG`).

#### 3. Hậu điều kiện (Post-conditions)
- **Thành công**: Người dùng xác thực thành công, được cấp token phiên làm việc (JWT) và hệ thống chuyển hướng tới màn hình chức năng tương ứng theo vai trò.
- **Thất bại**: Hệ thống giữ nguyên màn hình đăng nhập, hiển thị thông báo lỗi tương ứng và ghi nhận lịch sử đăng nhập thất bại. Nếu nhập sai 5 lần liên tiếp, tài khoản bị tạm khóa.

#### 4. Dòng sự kiện chính (Main Flow)
1. Người dùng mở giao diện đăng nhập hệ thống.
2. Người dùng nhập tên đăng nhập, mật khẩu và nhấn "Đăng nhập".
3. Hệ thống kiểm tra thông tin và xác thực tài khoản với cơ sở dữ liệu.
4. Hệ thống phân quyền tương ứng theo vai trò (`QUAN_LY` hoặc `NHAN_VIEN`), tạo token phiên làm việc và chuyển hướng vào màn hình làm việc chính.

#### 5. Các dòng sự kiện khác (Alternative Flows)
- **2a. Sai thông tin đăng nhập**:
  - Hệ thống kiểm tra thấy mật khẩu không đúng hoặc tài khoản không tồn tại.
  - Hệ thống ghi nhận số lần nhập sai vào nhật ký.
  - Nếu số lần sai liên tiếp đạt 5 lần: Hệ thống tự động khóa tài khoản tạm thời (`trang_thai = 'KHOA'`).
  - Hệ thống hiển thị thông báo lỗi và yêu cầu người dùng nhập lại.

#### 6. Các yêu cầu đặc biệt
- Mật khẩu phải được che/ẩn khi nhập trên giao diện.
- Mật khẩu lưu trữ trong cơ sở dữ liệu bắt buộc phải được mã hóa/băm bằng thuật toán an toàn (BCrypt).
- Tự động khóa tài khoản tạm thời khi nhập sai mật khẩu 5 lần liên tiếp.

#### 7. Điểm mở rộng
- Không có.

---

### UC02: ĐẶC TẢ USE CASE CHẤM CÔNG (IN/OUT)

#### 1. Thông tin chung
- **Mã Use Case**: UC02
- **Tên Use Case**: Chấm công (In/Out)
- **Actor chính**: Nhân viên, Quản lý
- **Tóm tắt**: Nhân viên hoặc Quản lý ghi nhận giờ vào ca (Check-in) và giờ tan ca (Check-out) hàng ngày để theo dõi chuyên cần và làm căn cứ tính công.

#### 2. Tiền điều kiện (Pre-conditions)
1. Người dùng đã đăng nhập thành công vào hệ thống.
2. Để thực hiện thao tác "Tan ca" (Check-out), người dùng bắt buộc phải đã thực hiện "Vào ca" (Check-in) trước đó trong ngày.

#### 3. Hậu điều kiện (Post-conditions)
- **Thành công**: Lịch sử chấm công (mốc thời gian Check-in / Check-out) được lưu vào cơ sở dữ liệu; trạng thái ca làm việc của nhân viên được cập nhật.
- **Thất bại**: Trạng thái chấm công không đổi, hệ thống hiển thị thông báo lỗi.

#### 4. Dòng sự kiện chính (Main Flow)
1. Người dùng truy cập chức năng "Chấm công (In/Out)".
2. Hệ thống kiểm tra thông tin phân công, trạng thái chấm công hiện tại và hiển thị giờ hệ thống.
3. Người dùng nhấn nút "Vào ca" (Check-in) hoặc "Tan ca" (Check-out).
4. Hệ thống lưu mốc thời gian thực tế từ đồng hồ máy chủ vào CSDL và cập nhật trạng thái làm việc.

#### 5. Các dòng sự kiện khác (Alternative Flows)
- **3a. Đã chấm công vào ca trước đó**:
  - Hệ thống kiểm tra thấy đã có mốc Check-in chưa đóng -> Tự động ẩn nút "Vào ca", chỉ bật nút "Tan ca".
- **Lưu dữ liệu thất bại**:
  - Hệ thống mất kết nối CSDL -> Trả về lỗi, hiển thị thông báo "Không thể ghi nhận chấm công, vui lòng thử lại". Trạng thái làm việc không thay đổi.

#### 6. Các yêu cầu đặc biệt
- Giờ chấm công phải được lấy từ thời gian thực của máy chủ (Server/System Time), không cho phép client/người dùng tự điều chỉnh thời gian.

#### 7. Điểm mở rộng
- Không có.

---

### UC03: ĐẶC TẢ USE CASE MỞ/CHỐT CA LÀM VIỆC

#### 1. Thông tin chung
- **Mã Use Case**: UC03
- **Tên Use Case**: Mở/Chốt ca làm việc
- **Actor chính**: Nhân viên (Thu ngân), Quản lý
- **Tóm tắt**: Thu ngân kiểm đếm tiền mặt đầu ca khi mở ca, hệ thống theo dõi doanh thu trong ca và đối soát số dư két khi chốt ca bàn giao.

#### 2. Tiền điều kiện (Pre-conditions)
1. Người dùng đã đăng nhập và đã hoàn thành chấm công vào ca.
2. Mỗi máy POS chỉ được phép tồn tại 1 ca làm việc ở trạng thái mở tại một thời điểm.
3. Khi thực hiện "Chốt ca": Bắt buộc đang có ca ở trạng thái đang mở (`DANG_MO`).

#### 3. Hậu điều kiện (Post-conditions)
- **Thành công**: Phiên ca được mở/chốt thành công, lưu số dư đầu/cuối ca, in phiếu bàn giao ca và ghi nhận chênh lệch kèm lý do giải trình (nếu có).
- **Thất bại**: Trạng thái phiên ca không thay đổi, hệ thống yêu cầu xử lý các vướng mắc (như giải trình tiền lệch) trước khi chốt ca.

#### 4. Dòng sự kiện chính (Main Flow)
1. Thu ngân mở chức năng "Mở/Chốt ca" trên phần mềm POS.
2. Đầu ca, thu ngân đếm tiền trong két, nhập số tiền mặt bàn giao ban đầu và nhấn "Mở ca".
3. Trong ca, hệ thống tự động cộng dồn doanh thu theo từng hình thức thanh toán phát sinh từ các đơn hàng.
4. Cuối ca, thu ngân kiểm đếm tiền mặt thực tế trong két và nhấn "Chốt ca".
5. Hệ thống đối chiếu số liệu tiền mặt thực tế với doanh thu tính toán trên hệ thống, in phiếu bàn giao ca và đóng phiên làm việc (`DA_CHOT`).

#### 5. Các dòng sự kiện khác (Alternative Flows)
- **4a. Sai lệch tiền mặt két (Tiền thực tế != Doanh thu hệ thống)**:
  - Hệ thống phát hiện chênh lệch và hiển thị thông báo yêu cầu thu ngân nhập lý do giải trình.
  - *Nếu thu ngân hủy hoặc không giải trình*: Hệ thống giữ nguyên trạng thái ca `DANG_MO` (hoặc `CHO_XU_LY_CHENH_LECH`), chưa cho chốt ca.
  - *Nếu thu ngân nhập lý do giải trình hợp lệ*: Hệ thống lưu số dư thực tế, lưu số tiền chênh lệch, lưu nội dung giải trình và xác nhận chốt ca thành công (khóa số liệu ca).

#### 6. Các yêu cầu đặc biệt
- Mỗi máy POS chỉ mở 1 ca tại 1 thời điểm.
- Ca làm việc sau khi đã chốt (`DA_CHOT`) thì bị khóa toàn bộ số liệu, tuyệt đối không được chỉnh sửa.

#### 7. Điểm mở rộng
- Thực hiện liên kết tới ca sử dụng Báo cáo ca làm việc khi in phiếu bàn giao ca.

---

### UC04: ĐẶC TẢ USE CASE TẠO/CHỈNH SỬA ĐƠN HÀNG

#### 1. Thông tin chung
- **Mã Use Case**: UC04
- **Tên Use Case**: Tạo/Chỉnh sửa đơn hàng
- **Actor chính**: Nhân viên
- **Tóm tắt**: Nhân viên chọn đồ lưu niệm, nhập hoặc điều chỉnh số lượng mua, hệ thống kiểm tra tồn kho, tính tổng tiền tạm tính và chuyển sang thanh toán.

#### 2. Tiền điều kiện (Pre-conditions)
- Người dùng đã đăng nhập và đang có ca làm việc ở trạng thái mở (`DANG_MO`).

#### 3. Hậu điều kiện (Post-conditions)
- **Thành công**: Danh sách sản phẩm, số lượng và tổng giá trị đơn hàng tạm tính được khởi tạo/cập nhật, sẵn sàng cho thanh toán hoặc lưu nháp.
- **Thất bại**: Giỏ hàng tạm giữ nguyên trạng thái trước khi thực hiện thao tác lỗi, hiển thị cảnh báo tương ứng.

#### 4. Dòng sự kiện chính (Main Flow)
1. Nhân viên mở màn hình Bán hàng POS.
2. Nhân viên quét mã vạch bằng máy quét hoặc gõ tìm kiếm theo mã/tên đồ lưu niệm.
3. Nhân viên nhập số lượng mua; hệ thống kiểm tra tồn kho khả dụng và tự động tính thành tiền.
4. Nhân viên có thể điều chỉnh số lượng mua hoặc xóa bớt món khỏi danh sách nếu khách đổi ý.
5. Nhân viên chốt danh sách sản phẩm và nhấn nút chuyển sang thanh toán.

#### 5. Các dòng sự kiện khác (Alternative Flows)
- **2a. Không tìm thấy sản phẩm**:
  - Hệ thống báo lỗi mã vạch không hợp lệ hoặc không có sản phẩm; nhân viên nhập lại mã hoặc tìm theo danh mục.
- **3a. Vượt quá số lượng tồn kho**:
  - Hệ thống cảnh báo số lượng mua vượt tồn kho thực tế; yêu cầu nhân viên giảm số lượng mua theo tồn khả dụng.
- **4a. Khách đổi ý điều chỉnh món**:
  - Nhân viên sửa số lượng hoặc xóa món; hệ thống tính toán lại tổng tiền tạm tính.
- **5a. Hủy đơn hàng**:
  - Nhân viên nhấn nút "Hủy đơn"; hệ thống xóa trắng giỏ hàng tạm, không lưu đơn và làm mới màn hình POS.

#### 6. Các yêu cầu đặc biệt
- Hỗ trợ thiết bị máy quét mã vạch (Barcode Scanner).
- Tuyệt đối không cho phép tạo đơn với số lượng vượt quá số lượng tồn kho thực tế.

#### 7. Điểm mở rộng
- Có thể mở rộng tới các ca sử dụng:
  - **Áp dụng giảm giá (UC05)** (`<<extend>>`)
  - **Lưu nháp đơn (UC06)** (`<<extend>>`)
  - **Thanh toán (UC07)** (`<<include>>`)

---

### UC05: ĐẶC TẢ USE CASE ÁP DỤNG GIẢM GIÁ

#### 1. Thông tin chung
- **Mã Use Case**: UC05
- **Tên Use Case**: Áp dụng giảm giá
- **Actor chính**: Nhân viên
- **Tóm tắt**: Ca sử dụng mở rộng (`<<extend>>`) cho phép nhân viên nhập mã voucher hoặc chọn mức chiết khấu để giảm trừ tiền trực tiếp trên đơn hàng.

#### 2. Tiền điều kiện (Pre-conditions)
1. Được mở rộng từ ca sử dụng Tạo/Chỉnh sửa đơn hàng (UC04).
2. Đang có đơn hàng ở trạng thái chưa thanh toán và có ít nhất 1 sản phẩm trong giỏ hàng.

#### 3. Hậu điều kiện (Post-conditions)
- **Thành công**: Đơn hàng được ghi nhận mức tiền giảm giá, tổng tiền thanh toán được cập nhật giảm trừ tương ứng.
- **Thất bại**: Mã giảm giá bị từ chối, tổng tiền đơn hàng giữ nguyên như ban đầu.

#### 4. Dòng sự kiện chính (Main Flow)
1. Nhân viên nhấn chọn "Áp dụng giảm giá" trên đơn hàng đang lập.
2. Nhân viên nhập chuỗi mã code voucher hoặc chọn mức chiết khấu (% hoặc số tiền).
3. Hệ thống kiểm tra điều kiện áp dụng của mã giảm giá (thời hạn, số lượt dùng, giá trị đơn tối thiểu).
4. Hệ thống tự động tính số tiền được giảm trừ và cập nhật lại tổng tiền thanh toán mới của đơn hàng.

#### 5. Các dòng sự kiện khác (Alternative Flows)
- **3a. Mã không hợp lệ hoặc hết hạn**:
  - Mã không tồn tại, hết hạn, vượt quá lượt dùng hoặc giá trị đơn chưa đạt mức tối thiểu.
  - Hệ thống hiển thị thông báo lỗi cụ thể, từ chối áp dụng và giữ nguyên tổng tiền ban đầu của đơn hàng.

#### 6. Các yêu cầu đặc biệt
- Mỗi đơn hàng chỉ được áp dụng tối đa một mã giảm giá.
- Số tiền giảm giá không được vượt quá tổng giá trị đơn hàng.

#### 7. Điểm mở rộng
- Là điểm mở rộng (`<<extend>>`) từ Use Case UC04 (Tạo/Chỉnh sửa đơn hàng).

---

### UC06: ĐẶC TẢ USE CASE LƯU NHÁP ĐƠN

#### 1. Thông tin chung
- **Mã Use Case**: UC06
- **Tên Use Case**: Lưu nháp đơn
- **Actor chính**: Nhân viên
- **Tóm tắt**: Ca sử dụng mở rộng (`<<extend>>`) tạm lưu giỏ hàng hiện tại vào danh sách đơn nháp khi khách cần đi lựa thêm đồ, giúp giải phóng quầy thu ngân phục vụ khách tiếp theo mà không bị mất dữ liệu.

#### 2. Tiền điều kiện (Pre-conditions)
1. Điểm mở rộng (`<<extend>>`) từ ca sử dụng UC04 (Tạo/Chỉnh sửa đơn hàng).
2. Đơn hàng hiện tại có ít nhất 1 sản phẩm và chưa thanh toán.
3. Đơn nháp chưa được trừ vào tồn kho thực tế.

#### 3. Hậu điều kiện (Post-conditions)
- **Thành công**: Đơn hàng được lưu vào danh sách đơn nháp (`trang_thai = 'LUU_NHAP'`), màn hình POS được xóa trắng để tạo đơn hàng mới.
- **Thất bại**: Đơn hàng không lưu được, thông tin giỏ hàng giữ nguyên trên màn hình.

#### 4. Dòng sự kiện chính (Main Flow)
1. Nhân viên nhấn nút "Lưu nháp đơn" khi khách tạm hoãn để lựa thêm quà.
2. Hệ thống lưu giỏ hàng tạm thời và làm mới (xóa sạch) màn hình bán hàng.
3. Khi khách quay lại quầy, nhân viên mở mục "Danh sách đơn nháp" và chọn đơn tương ứng.
4. Hệ thống nạp lại toàn bộ danh sách sản phẩm của đơn nháp lên màn hình để tiếp tục chỉnh sửa hoặc thanh toán.

#### 5. Các dòng sự kiện khác (Alternative Flows)
- **1a. Đơn hàng rỗng**:
  - Giỏ hàng chưa có sản phẩm nào -> Hệ thống thông báo không thể lưu nháp đơn rỗng.
- **3a. Khách bỏ đi / Hủy mua**:
  - Khách không quay lại -> Nhân viên chọn đơn nháp đó và bấm "Xóa đơn nháp", hệ thống hủy bản ghi tạm.

#### 6. Các yêu cầu đặc biệt
- Đơn nháp tuyệt đối không được trừ tồn kho; hàng trong kho chỉ thực sự bị trừ khi đơn chuyển sang "Đã thanh toán".

#### 7. Điểm mở rộng
- Là điểm mở rộng (`<<extend>>`) từ Use Case UC04.

---

### UC07: ĐẶC TẢ USE CASE THANH TOÁN

#### 1. Thông tin chung
- **Mã Use Case**: UC07
- **Tên Use Case**: Thanh toán
- **Actor chính**: Nhân viên, Cổng thanh toán
- **Tóm tắt**: Ca sử dụng bao hàm (`<<include>>`) tiếp nhận thanh toán từ khách qua Tiền mặt hoặc Chuyển khoản QR, tự động trừ tồn kho và in hóa đơn giao dịch.

#### 2. Tiền điều kiện (Pre-conditions)
1. Đơn hàng đã được chốt danh sách sản phẩm và ca làm việc đang ở trạng thái mở (`DANG_MO`).
2. Thanh toán bằng hình thức chuyển khoản yêu cầu hệ thống có kết nối Internet với Cổng thanh toán.
3. Bao hàm từ UC04; có thể mở rộng sang UC10 (Tích điểm / Đổi điểm).

#### 3. Hậu điều kiện (Post-conditions)
- **Thành công**: Đơn hàng chuyển sang trạng thái "Đã thanh toán" (`DA_THANH_TOAN`), ghi nhận doanh thu vào ca, trừ số lượng tồn kho của các sản phẩm tương ứng và in hóa đơn.
- **Thất bại**: Đơn hàng giữ nguyên trạng thái "Chờ thanh toán", không trừ tồn kho, thông báo lỗi phương thức thanh toán.

#### 4. Dòng sự kiện chính (Main Flow)
1. Hệ thống hiển thị tổng số tiền khách cần thanh toán của đơn hàng.
2. Nhân viên lựa chọn phương thức thanh toán: Tiền mặt hoặc Chuyển khoản (quét mã QR).
3. Nếu khách trả **Tiền mặt**: Nhân viên nhập số tiền khách đưa; hệ thống tự động tính và hiển thị số tiền thừa cần trả lại khách.
4. Nếu khách trả **Chuyển khoản**: Hệ thống gửi yêu cầu tới Cổng thanh toán để tạo mã VietQR động chứa chính xác số tiền đơn hàng và hiển thị lên màn hình.
5. Nhân viên xác nhận đã nhận đủ tiền và nhấn "Xác nhận thanh toán".
6. Hệ thống cập nhật trạng thái đơn thành "Đã thanh toán", tự động trừ tồn kho các mặt hàng trong CSDL và gửi lệnh in hóa đơn.

#### 5. Các dòng sự kiện khác (Alternative Flows)
- **2a. Đổi hình thức thanh toán**: Nhân viên có thể chuyển đổi qua lại giữa tiền mặt và chuyển khoản QR trước khi bấm xác nhận.
- **Thanh toán thất bại**: Mất kết nối cổng thanh toán hoặc khách chưa chuyển đủ tiền -> Hệ thống báo lỗi, đơn hàng giữ ở trạng thái "Chờ thanh toán".
- **6a. Máy in hết giấy hoặc lỗi kết nối máy in**: Đơn hàng đã ghi nhận thành công và kho đã trừ; hệ thống cảnh báo máy in lỗi, nhân viên thay giấy và chọn "In lại hóa đơn".

#### 6. Các yêu cầu đặc biệt
- Mã QR động phải chứa đúng số tiền thanh toán của đơn hàng để tránh khách chuyển sai số tiền.
- Chỉ khi nhận được tín hiệu thanh toán thành công mới tiến hành trừ tồn kho và in hóa đơn.

#### 7. Điểm mở rộng
- Mở rộng (`<<extend>>`) tới ca sử dụng Tích điểm / Đổi điểm (UC10) nếu khách hàng là hội viên.

---

### UC08: ĐẶC TẢ USE CASE TRẢ HÀNG / HOÀN TIỀN

#### 1. Thông tin chung
- **Mã Use Case**: UC08
- **Tên Use Case**: Trả hàng / Hoàn tiền
- **Actor chính**: Quản lý, Cổng thanh toán
- **Tóm tắt**: Quản lý tiếp nhận hàng trả từ khách, tra cứu hóa đơn gốc, tính tiền hoàn trả, hoàn tiền cho khách và tự động cộng hoàn số lượng vào tồn kho.

#### 2. Tiền điều kiện (Pre-conditions)
1. Người dùng đã đăng nhập với vai trò Quản lý (`QUAN_LY`).
2. Hóa đơn mua hàng gốc đã ở trạng thái "Đã thanh toán" và còn lưu trên hệ thống.

#### 3. Hậu điều kiện (Post-conditions)
- **Thành công**: Phiếu trả hàng / hoàn tiền được lập, ghi nhận giảm trừ doanh thu trong báo cáo, số lượng sản phẩm được cộng trả lại vào tồn kho thực tế.
- **Thất bại**: Không xuất phiếu trả, giữ nguyên dữ liệu hóa đơn gốc và tồn kho.

#### 4. Dòng sự kiện chính (Main Flow)
1. Quản lý mở chức năng "Trả hàng / Hoàn tiền".
2. Quản lý nhập mã hóa đơn để tra cứu thông tin đơn mua gốc.
3. Quản lý chọn sản phẩm cần trả, nhập số lượng trả và kiểm tra tình trạng hàng hóa thực tế.
4. Hệ thống kiểm tra điều kiện đổi trả, tính tiền hoàn; Quản lý nhấn "Xác nhận hoàn tiền".
5. Hệ thống tạo phiếu trả hàng, tự động cộng lại tồn kho và hoàn tiền cho khách (tiền mặt hoặc qua cổng thanh toán).

#### 5. Các dòng sự kiện khác (Alternative Flows)
- **2a. Tra cứu hóa đơn gốc**:
  - Không tìm thấy theo mã hóa đơn: Quản lý tra cứu lại theo Số điện thoại khách hàng hoặc ngày mua.
  - Không tìm thấy bất kỳ hóa đơn nào: Hệ thống báo lỗi và dừng xử lý.
- **3a. Hàng không đủ điều kiện trả**:
  - Hàng quá hạn đổi trả, bị hư hỏng do lỗi của khách hoặc số lượng trả vượt quá số lượng đã mua.
  - Quản lý từ chối đổi trả theo chính sách và kết thúc Use Case.
- **Hoàn tiền qua chuyển khoản**:
  - Nếu hóa đơn gốc trả bằng chuyển khoản/ví: Hệ thống gửi yêu cầu hoàn tiền tương ứng qua cổng thanh toán.

#### 6. Các yêu cầu đặc biệt
- Chỉ tiếp nhận đổi trả cho hàng hóa còn trong thời hạn quy định và có hóa đơn hợp lệ.
- Số lượng hàng trả của từng món không được vượt quá số lượng khách đã mua trên hóa đơn gốc.

#### 7. Điểm mở rộng
- Là điểm mở rộng (`<<extend>>`) tới ca sử dụng Thanh toán (UC07) hoặc xem lại lịch sử hóa đơn.

---

### UC09: ĐẶC TẢ USE CASE QUẢN LÝ HỒ SƠ KHÁCH HÀNG

#### 1. Thông tin chung
- **Mã Use Case**: UC09
- **Tên Use Case**: Quản lý hồ sơ khách hàng
- **Actor chính**: Nhân viên, Quản lý
- **Tóm tắt**: Nhân viên hoặc Quản lý tra cứu thông tin khách hàng qua Số điện thoại, tạo mới hồ sơ khách thân thiết và theo dõi lịch sử mua hàng cùng điểm thưởng tích lũy.

#### 2. Tiền điều kiện (Pre-conditions)
1. Người dùng đã đăng nhập vào hệ thống.
2. Mỗi khách hàng có một Số điện thoại duy nhất, không được trùng lặp.

#### 3. Hậu điều kiện (Post-conditions)
- **Thành công**: Hồ sơ khách hàng được tạo mới hoặc cập nhật trong CSDL, sẵn sàng gắn liền với các đơn hàng để tích/đổi điểm.
- **Thất bại**: Dữ liệu khách hàng giữ nguyên, hệ thống thông báo lỗi trùng lặp hoặc sai định dạng.

#### 4. Dòng sự kiện chính (Main Flow)
1. Người dùng mở chức năng "Quản lý khách hàng".
2. Người dùng nhập Số điện thoại để tìm kiếm hồ sơ khách hàng.
3. Nếu khách hàng đã có hồ sơ, hệ thống hiển thị thông tin, điểm tích lũy và lịch sử mua sắm.
4. Nếu là khách mới, người dùng nhập họ tên, số điện thoại và nhấn "Lưu hồ sơ".
5. Hệ thống kiểm tra tính hợp lệ và lưu thông tin khách hàng vào cơ sở dữ liệu.

#### 5. Các dòng sự kiện khác (Alternative Flows)
- **2a. Khách vãng lai**: Nhân viên chọn đơn cho "Khách vãng lai", bỏ qua bước lưu hồ sơ khách hàng.
- **4a. Số điện thoại không hợp lệ**: SĐT không đúng 10 chữ số hoặc đã tồn tại trong CSDL -> Hệ thống báo lỗi và yêu cầu nhập lại đúng thông tin.

#### 6. Các yêu cầu đặc biệt
- Số điện thoại là trường định danh duy nhất (Unique).
- Thông tin cá nhân của khách hàng phải được bảo mật.

#### 7. Điểm mở rộng
- Có thể mở rộng từ UC04 (Tạo/Chỉnh sửa đơn hàng) khi cần gán thông tin khách hàng vào đơn.

---

### UC10: ĐẶC TẢ USE CASE TÍCH ĐIỂM / ĐỔI ĐIỂM

#### 1. Thông tin chung
- **Mã Use Case**: UC10
- **Tên Use Case**: Tích điểm / Đổi điểm
- **Actor chính**: Nhân viên
- **Tóm tắt**: Ca sử dụng mở rộng (`<<extend>>`) tự động tích điểm thưởng theo % giá trị đơn sau khi thanh toán thành công hoặc quy đổi điểm tích lũy sẵn có thành tiền giảm trừ trực tiếp trên hóa đơn.

#### 2. Tiền điều kiện (Pre-conditions)
1. Điểm mở rộng (`<<extend>>`) từ ca sử dụng Thanh toán (UC07).
2. Đơn hàng đang ở bước thanh toán.
3. Khách hàng đã có hồ sơ thành viên (không áp dụng đối với khách vãng lai).

#### 3. Hậu điều kiện (Post-conditions)
- **Thành công**: Quỹ điểm thưởng của khách được trừ (khi đổi điểm) hoặc cộng thêm (sau khi thanh toán); điểm thưởng mới hiển thị trên hóa đơn.
- **Thất bại**: Quỹ điểm giữ nguyên, hệ thống thông báo lỗi không đủ điều kiện tích/đổi điểm.

#### 4. Dòng sự kiện chính (Main Flow)
1. Tại bước thanh toán, hệ thống tra cứu và hiển thị điểm tích lũy của khách hàng theo Số điện thoại.
2. Nếu khách chọn đổi điểm: Nhân viên nhập số điểm cần dùng để quy đổi giảm trừ trực tiếp vào tiền thanh toán.
3. Khách hoàn tất thanh toán đơn hàng (UC07).
4. Sau khi thanh toán thành công, hệ thống tự động tính điểm mới theo % giá trị đơn, cộng vào quỹ điểm của khách và in thông tin điểm lên hóa đơn.

#### 5. Các dòng sự kiện khác (Alternative Flows)
- **2a. Chưa đủ điểm đổi**:
  - Số điểm tích lũy chưa đạt hạn mức tối thiểu để quy đổi -> Hệ thống thông báo chưa đủ điều kiện và giữ nguyên tổng tiền chuẩn.
- **2b. Khách muốn giữ điểm**:
  - Khách không muốn dùng điểm -> Nhân viên bỏ qua bước đổi điểm; hệ thống chỉ cộng điểm mới sau khi thanh toán xong.
- **Thanh toán đơn hàng thất bại**:
  - Không cộng điểm mới và không trừ điểm đổi do đơn hàng chưa thanh toán thành công.

#### 6. Các yêu cầu đặc biệt
- Tỷ lệ tích điểm (ví dụ 5%) và tỷ lệ quy đổi điểm (ví dụ 1.000 điểm = 10.000đ) do Quản lý thiết lập trong bảng tham số.
- Chỉ cộng điểm tích lũy sau khi đơn hàng được ghi nhận thanh toán thành công.

#### 7. Điểm mở rộng
- Là điểm mở rộng (`<<extend>>`) từ ca sử dụng Thanh toán (UC07); bao hàm (`<<include>>`) ca sử dụng Quản lý hồ sơ khách hàng (UC09).

---

### UC11: ĐẶC TẢ USE CASE QUẢN LÝ SẢN PHẨM (CRUD)

#### 1. Thông tin chung
- **Mã Use Case**: UC11
- **Tên Use Case**: Quản lý sản phẩm (CRUD)
- **Actor chính**: Quản lý, Nhân viên (Quản lý toàn quyền CRUD, Nhân viên xem/tra cứu)
- **Tóm tắt**: Quản lý thêm mới, sửa thông tin, xóa/chuyển trạng thái ngừng kinh doanh và tra cứu các mặt hàng đồ lưu niệm theo mã SP, tên, giá nhập, giá bán và số lượng tồn.

#### 2. Tiền điều kiện (Pre-conditions)
1. Người dùng đã đăng nhập với vai trò Quản lý (`QUAN_LY`).
2. Mã sản phẩm không được trùng; đơn giá và số lượng không được mang giá trị âm.

#### 3. Hậu điều kiện (Post-conditions)
- **Thành công**: Thông tin sản phẩm trong danh mục được tạo mới, cập nhật hoặc chuyển sang trạng thái ngừng kinh doanh trong CSDL.
- **Thất bại**: Dữ liệu sản phẩm không đổi, hiển thị thông báo lỗi dữ liệu không hợp lệ.

#### 4. Dòng sự kiện chính (Main Flow)
1. Quản lý mở màn hình "Quản lý sản phẩm".
2. Hệ thống tải và hiển thị danh mục sản phẩm lưu niệm hiện có.
3. Quản lý chọn thao tác: Thêm mới, Sửa thông tin hoặc Xóa/Ngừng kinh doanh.
4. Quản lý nhập thông tin chi tiết (mã SP, tên món, danh mục, đơn vị tính, giá nhập, giá bán, tồn kho) và nhấn "Lưu".
5. Hệ thống kiểm tra dữ liệu hợp lệ, lưu vào cơ sở dữ liệu và thông báo thành công.

#### 5. Các dòng sự kiện khác (Alternative Flows)
- **4a. Trùng mã sản phẩm hoặc dữ liệu âm**:
  - Mã SP đã tồn tại hoặc giá/số lượng < 0 -> Hệ thống báo lỗi và yêu cầu nhập lại.
- **4b. Xóa sản phẩm đã từng phát sinh giao dịch bán hàng / nhập kho**:
  - Hệ thống kiểm tra lịch sử giao dịch -> Chặn xóa vật lý để bảo toàn dữ liệu liên kết, tự động chuyển trạng thái sản phẩm sang "Ngừng kinh doanh" (`trang_thai = 'NGUNG_KINH_DOANH'`).
  - Nếu sản phẩm mới tạo và chưa có giao dịch: Thực hiện xóa hoàn toàn bản ghi khỏi CSDL.

#### 6. Các yêu cầu đặc biệt
- Mã sản phẩm (SKU) phải duy nhất để phục vụ quét mã vạch.
- Giá nhập, giá bán và số lượng tồn kho bắt buộc >= 0.

#### 7. Điểm mở rộng
- Không có.

---

### UC12: ĐẶC TẢ USE CASE TẠO PHIẾU NHẬP KHO

#### 1. Thông tin chung
- **Mã Use Case**: UC12
- **Tên Use Case**: Tạo phiếu nhập kho
- **Actor chính**: Quản lý
- **Tóm tắt**: Quản lý lập phiếu nhập hàng khi mua thêm đồ lưu niệm từ đối tác nhà cung cấp để tự động cộng tăng số lượng tồn kho trên hệ thống.

#### 2. Tiền điều kiện (Pre-conditions)
1. Người dùng đã đăng nhập với vai trò Quản lý (`QUAN_LY`).
2. Nhà cung cấp và các sản phẩm cần nhập đã tồn tại trong danh mục hệ thống.
3. Phiếu nhập sau khi xác nhận sẽ không được chỉnh sửa hoặc xóa.

#### 3. Hậu điều kiện (Post-conditions)
- **Thành công**: Phiếu nhập kho được lưu vào hệ thống, số lượng tồn kho của các sản phẩm tương ứng được tự động cộng tăng.
- **Thất bại**: Phiếu nhập không được lưu, số lượng tồn kho giữ nguyên.

#### 4. Dòng sự kiện chính (Main Flow)
1. Quản lý mở chức năng "Tạo phiếu nhập kho".
2. Quản lý chọn nhà cung cấp và chọn các sản phẩm cần nhập từ danh mục.
3. Quản lý nhập số lượng nhập và đơn giá nhập cho từng sản phẩm; hệ thống tự tính tổng tiền nhập.
4. Quản lý kiểm tra thông tin và nhấn "Xác nhận nhập kho".
5. Hệ thống lưu phiếu nhập vào CSDL và tự động cộng tăng số lượng tồn kho tương ứng của từng mặt hàng.

#### 5. Các dòng sự kiện khác (Alternative Flows)
- **2a. Mặt hàng mới (chưa có trong danh mục)**:
  - Quản lý thực hiện tạo nhanh thông tin sản phẩm mới trước khi đưa vào danh sách của phiếu nhập kho (liên kết UC11).
- **Lỗi lưu dữ liệu**:
  - Lỗi kết nối CSDL -> Hệ thống hiển thị thông báo lỗi, phiếu không lưu và tồn kho không đổi.

#### 6. Các yêu cầu đặc biệt
- Phiếu nhập kho đã ở trạng thái xác nhận (`DA_NHAP`) thì bị khóa, không được sửa hoặc xóa để bảo đảm tính toàn vẹn của sổ sách kế toán kho.

#### 7. Điểm mở rộng
- Có thể liên kết mở rộng tới ca sử dụng Quản lý sản phẩm (UC11) khi cần tạo nhanh sản phẩm mới.

---

### UC13: ĐẶC TẢ USE CASE KIỂM KÊ KHO

#### 1. Thông tin chung
- **Mã Use Case**: UC13
- **Tên Use Case**: Kiểm kê kho
- **Actor chính**: Quản lý
- **Tóm tắt**: Quản lý đối chiếu số lượng sản phẩm thực tế tại quầy/kho với dữ liệu sổ sách trên phần mềm, ghi nhận chênh lệch, yêu cầu giải trình và cân bằng lại tồn kho.

#### 2. Tiền điều kiện (Pre-conditions)
1. Người dùng đã đăng nhập với vai trò Quản lý (`QUAN_LY`).
2. Yêu cầu bắt buộc: Phải nhập lý do giải trình khi số lượng kiểm đếm thực tế khác với số lượng trên sổ sách.

#### 3. Hậu điều kiện (Post-conditions)
- **Thành công**: Phiếu kiểm kê được lưu trữ, số lượng tồn kho sổ sách được điều chỉnh cân bằng về đúng số lượng thực tế kèm lý do chênh lệch.
- **Thất bại**: Dữ liệu tồn kho giữ nguyên, kết quả kiểm kê không được cập nhật.

#### 4. Dòng sự kiện chính (Main Flow)
1. Quản lý mở chức năng "Kiểm kê kho".
2. Hệ thống truy vấn và hiển thị danh sách sản phẩm cùng số lượng tồn sổ sách hiện tại.
3. Quản lý kiểm đếm thực tế bằng mắt tại quầy/kho và nhập số lượng tồn thực tế vào phần mềm.
4. Hệ thống tự động tính mức chênh lệch (Thừa / Thiếu / Khớp); nếu có chênh lệch, Quản lý nhập lý do giải trình.
5. Quản lý nhấn "Cân bằng tồn kho"; hệ thống cập nhật số lượng tồn kho sổ sách về đúng số lượng thực tế.

#### 5. Các dòng sự kiện khác (Alternative Flows)
- **3a. Tạm dừng kiểm kê**: Quản lý chọn "Lưu tạm" để lưu kết quả đã đếm và tiếp tục kiểm kê bổ sung sau.
- **Có chênh lệch nhưng thiếu lý do giải trình**: Hệ thống chặn thao tác cân bằng tồn kho, báo lỗi yêu cầu bắt buộc nhập giải trình (mất mát, vỡ hỏng, nhầm mã...).

#### 6. Các yêu cầu đặc biệt
- Bắt buộc phải có lý do giải trình đối với các mặt hàng có độ lệch giữa thực tế và sổ sách trước khi cho phép cân bằng kho.

#### 7. Điểm mở rộng
- Không có.

---

### UC14: ĐẶC TẢ USE CASE XEM BÁO CÁO (DOANH THU, KHO, CA)

#### 1. Thông tin chung
- **Mã Use Case**: UC14
- **Tên Use Case**: Xem báo cáo (Doanh thu, Kho, Ca)
- **Actor chính**: Quản lý
- **Tóm tắt**: Quản lý xem báo cáo doanh thu, lợi nhuận gộp theo ngày/tháng, phân tích sản phẩm bán chạy/bán chậm và đối soát ca làm việc của nhân viên; hỗ trợ xuất file Excel/PDF.

#### 2. Tiền điều kiện (Pre-conditions)
- Người dùng đã đăng nhập vào hệ thống với vai trò Quản lý (`QUAN_LY`).

#### 3. Hậu điều kiện (Post-conditions)
- **Thành công**: Hệ thống truy xuất, tổng hợp và hiển thị bảng biểu/biểu đồ báo cáo chính xác theo điều kiện lọc; hỗ trợ tải file báo cáo về máy.
- **Thất bại**: Báo lỗi kết nối dữ liệu hoặc không hiển thị được báo cáo.

#### 4. Dòng sự kiện chính (Main Flow)
1. Quản lý truy cập phân hệ "Báo cáo & Thống kê".
2. Quản lý chọn loại báo cáo: Doanh thu - Lợi nhuận, Báo cáo tồn kho hoặc Báo cáo ca làm việc.
3. Quản lý chọn khoảng thời gian cần lọc (theo ngày, tuần, tháng hoặc khoảng tùy chọn).
4. Hệ thống tổng hợp dữ liệu, hiển thị bảng số liệu chi tiết và biểu đồ trực quan.
5. Quản lý có thể nhấn "Xuất file" để tải báo cáo về máy (Excel hoặc PDF).

#### 5. Các dòng sự kiện khác (Alternative Flows)
- **Lỗi kết nối dữ liệu**: Hệ thống mất kết nối CSDL -> Báo lỗi kết nối và không hiển thị báo cáo.
- **3a. Không có giao dịch phát sinh trong kỳ**: Hệ thống hiển thị doanh thu = 0 kèm thông báo không phát sinh giao dịch.
- **5a. Xuất file**: Quản lý chọn định dạng xuất file (Excel hoặc PDF), hệ thống kết xuất và tải về máy.

#### 6. Các yêu cầu đặc biệt
- Số liệu doanh thu báo cáo phải khớp chính xác với tổng giá trị của các hóa đơn đã thanh toán (`DA_THANH_TOAN`) trong kỳ.
- Cho phép xuất báo cáo ra các định dạng chuẩn (Excel `.xlsx`, PDF).

#### 7. Điểm mở rộng
- Không có.

---

### UC15: ĐẶC TẢ USE CASE QUẢN LÝ NHÂN VIÊN & PHÂN QUYỀN

#### 1. Thông tin chung
- **Mã Use Case**: UC15
- **Tên Use Case**: Quản lý nhân viên & Phân quyền
- **Actor chính**: Quản lý
- **Tóm tắt**: Quản lý tạo tài khoản nhân sự mới, lưu trữ thông tin cá nhân và thiết lập phân quyền 2 cấp: Cấp Nhân viên (chỉ bán hàng, ca, chấm công) và Cấp Quản lý (toàn quyền hệ thống).

#### 2. Tiền điều kiện (Pre-conditions)
1. Người dùng đã đăng nhập với vai trò Quản lý (`QUAN_LY`).
2. Tên đăng nhập của nhân viên là duy nhất trong toàn hệ thống.
3. Hệ thống luôn phải duy trì ít nhất 1 tài khoản có vai trò Quản lý đang hoạt động.

#### 3. Hậu điều kiện (Post-conditions)
- **Thành công**: Thông tin nhân sự và quyền truy cập của tài khoản được tạo/cập nhật thành công trong CSDL.
- **Thất bại**: Quyền và thông tin tài khoản không thay đổi, báo lỗi tương ứng.

#### 4. Dòng sự kiện chính (Main Flow)
1. Quản lý mở phân hệ "Quản lý nhân viên & Phân quyền".
2. Hệ thống truy vấn và hiển thị danh sách nhân viên cùng trạng thái tài khoản.
3. Quản lý chọn thao tác: Thêm nhân viên mới, Sửa thông tin hoặc Phân quyền tài khoản.
4. Quản lý nhập thông tin cá nhân, gán vai trò (`QUAN_LY` hoặc `NHAN_VIEN`) và nhấn "Lưu".
5. Hệ thống kiểm tra hợp lệ và cập nhật thông tin nhân viên vào cơ sở dữ liệu.

#### 5. Các dòng sự kiện khác (Alternative Flows)
- **4a. Nhân viên truy cập chức năng bị hạn chế**:
  - Tài khoản có vai trò `NHAN_VIEN` cố truy cập trang Quản lý / Báo cáo -> Hệ thống tự động chặn truy cập và thông báo không có quyền thực hiện.
- **3a. Trùng tên đăng nhập hoặc vi phạm điều kiện tối thiểu 1 Quản lý**:
  - Tên đăng nhập bị trùng hoặc cố tình xóa/hạ quyền của tài khoản Quản lý duy nhất còn lại -> Hệ thống báo lỗi và hủy thao tác.

#### 6. Các yêu cầu đặc biệt
- Tên đăng nhập là duy nhất và không được phép trùng.
- Tuyệt đối không cho phép xóa hoặc khóa tài khoản Quản lý cuối cùng đang hoạt động của hệ thống.

#### 7. Điểm mở rộng
- Không có.
