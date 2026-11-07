# HỆ THỐNG WEBSITE THƯƠNG MẠI ĐIỆN TỬ ĐIỆN MÁY XANH

Hệ thống website thương mại điện tử chuyên ngành thiết bị điện tử, điện gia dụng và công nghệ số được xây dựng trên nền tảng **ASP.NET Core 8.0 MVC** kết hợp **Entity Framework Core** và cơ sở dữ liệu **Microsoft SQL Server**, mô phỏng theo mô hình trải nghiệm và quy trình vận hành thực tế của chuỗi siêu thị bán lẻ Điện Máy Xanh.

---

## 1. TỔNG QUAN HỆ THỐNG

Dự án cung cấp một giải pháp phần mềm thương mại điện tử toàn diện bao gồm hai phân hệ độc lập:
1. **Phân hệ Cửa hàng Bán lẻ (Storefront UX)**: Phục vụ khách hàng tìm kiếm trực tiếp, so sánh thông số kỹ thuật, tương tác trợ lý ảo AI chatbot, chọn mua, áp dụng voucher ưu đãi, thanh toán quét mã VietQR tự động và tra cứu tiến độ đơn hàng.
2. **Phân hệ Quản trị Vận hành (Admin CMS)**: Cung cấp trung tâm điều hành tập trung cho nhà bán hàng để kiểm soát danh mục sản phẩm, quản lý kho hàng khép kín (nhập/xuất tự động, khóa sửa tay tồn kho), xử lý đơn hàng, quản lý mã giảm giá, in hóa đơn chuẩn A4 và xuất báo cáo tài chính định dạng PDF chuyên sâu.

---

## 2. CÔNG NGHỆ VÀ THƯ VIỆN SỬ DỤNG

### Backend & Cơ sở dữ liệu
- **Framework**: [.NET 8.0 (C# 12)](https://dotnet.microsoft.com/) - ASP.NET Core MVC.
- **ORM**: [Entity Framework Core 8.0.11](https://docs.microsoft.com/ef/core/) (Code-First với hệ thống Migration quản lý phiên bản cơ sở dữ liệu tự động).
- **Cơ sở dữ liệu**: Microsoft SQL Server (Hỗ trợ SQL Server 2019/2022, SQL Server Express, LocalDB).
- **Kiến trúc**: MVC (Model - View - Controller), Repository pattern thông qua `ApplicationDbContext`, Session-based Cart & Authentication.
- **Bảo mật & Ràng buộc dữ liệu**: AntiForgeryToken, DataAnnotations (Regex 10 chữ số chuẩn đầu số viễn thông Việt Nam `^0[35789][0-9]{8}$`), kiểm tra chống trùng lặp tài khoản độc lập cho Username, Email và Số điện thoại.

### Frontend & Giao diện người dùng
- **View Engine**: Razor View Engine (`.cshtml`) với cơ chế phân tách Layout độc lập:
  - `_Layout.cshtml`: Giao diện khách hàng tối ưu theo phong cách Điện Máy Xanh (Tone màu Xanh dương `#0071e3` và Vàng `#fed700`), tích hợp thanh Floating Dock, Chatbot AI và Modal yêu cầu tư vấn.
  - `_AdminLayout.cshtml`: Giao diện quản trị hiện đại (Dark Navy Sidebar `#0d1726` kết hợp Card content sáng màu).
  - `_AuthLayout.cshtml`: Giao diện tinh gọn dành riêng cho màn hình Đăng nhập / Đăng ký.
- **Framework CSS**: Bootstrap 5.3 + Bộ biểu tượng Bootstrap Icons (1.11.3).
- **Thư viện tương tác & Tiện ích mở rộng**:
  - `SweetAlert2 (v11)`: Hộp thoại thông báo Toast tự tắt và xác nhận thao tác an toàn.
  - `Chart.js`: Biểu đồ cột kép và biểu đồ tròn phân tích doanh thu theo ngày và cơ cấu ngành hàng.
  - `html2pdf.js (0.10.1)`: Kết xuất báo cáo và hóa đơn thành tệp tin PDF chuẩn A4 trực tiếp trên trình duyệt.
- **Tích hợp Cổng thanh toán & Dịch vụ số**:
  - `VietQR Dynamic API (img.vietqr.io)`: Tạo mã QR thanh toán động chuẩn Napas 247, tự động nạp Số tài khoản, Ngân hàng MBBank, Số tiền và Nội dung chuyển khoản theo mã đơn.
  - `API Đơn vị Hành chính Mới (provinces.open-api.vn)`: Cập nhật 34 Tỉnh/Thành phố và Phường/Xã với cơ chế Backend Proxy tránh lỗi CORS.

---

## 3. CẤU TRÚC THƯ MỤC DỰ ÁN

```text
WebsiteDienMay/
├── Controllers/                    # Bộ điều hướng xử lý nghiệp vụ
│   ├── AccountController.cs       # Xác thực, đăng ký, đăng nhập, kiểm tra trùng lặp, hồ sơ, hủy đơn
│   ├── AdminController.cs         # Trung tâm điều hành CMS, kho hàng, hóa đơn, báo cáo doanh thu PDF
│   ├── CartController.cs          # Giỏ hàng, kiểm tra tồn kho, áp dụng voucher, thanh toán VietQR
│   ├── HomeController.cs          # Trang chủ, giới thiệu, chính sách bảo hành
│   └── ProductController.cs       # Danh sách, chi tiết, tìm kiếm trực tiếp, so sánh sản phẩm
├── Data/                          # Cơ sở dữ liệu và khởi tạo dữ liệu mẫu
│   ├── ApplicationDbContext.cs   # Cấu hình DbSet, khóa ngoại và ràng buộc dữ liệu
│   └── DbInitializer.cs          # Tự động Migration và nạp dữ liệu mẫu ban đầu
├── Migrations/                    # Lịch sử các bản di chuyển cơ sở dữ liệu EF Core
│   ├── 20261106071258_InitialCreate.cs
│   ├── 20261107120936_AddWarehouseInventory.cs
│   └── 20261107125842_AddCouponsTable.cs
├── Models/                        # Định nghĩa thực thể và mô hình dữ liệu
│   ├── Entities.cs               # Category, Brand, Product, StockImport, Order, Coupon, User...
│   └── ViewModels.cs             # ProductList, Cart, Checkout, Warehouse, RevenueReport, Auth...
├── Views/                         # Giao diện người dùng Razor View
│   ├── Account/                  # Login, Register, Profile, AccessDenied
│   ├── Admin/                    # Dashboard, Products, Warehouse, Orders, Brands, Coupons, Reports...
│   ├── Cart/                     # Index, Checkout, OrderSuccess (VietQR), TrackOrder (Inline VietQR)
│   ├── Home/                     # Index, About, WarrantyPolicy, Privacy
│   ├── Product/                  # Index, Detail, Compare
│   └── Shared/                   # _Layout, _AdminLayout, _AuthLayout, _ProductCard...
├── wwwroot/                       # Tài nguyên tĩnh phía máy khách
│   ├── css/                      # site.css (Floating Dock, Chatbot, Badge), admin.css
│   ├── js/                       # site.js (Live search, Compare bar, Chatbot AI, Scroll top, Callback)
│   └── lib/                      # bootstrap, jquery
├── appsettings.json               # Cấu hình chuỗi kết nối và thông số ứng dụng
├── Program.cs                     # Cấu hình Dependency Injection và HTTP Pipeline
└── WebsiteDienMay.csproj          # Tệp cấu hình gói và môi trường dự án
```

---

## 4. KIẾN TRÚC DỮ LIỆU & CÁC THỰC THỂ CHÍNH

Hệ thống cơ sở dữ liệu được thiết kế theo chuẩn quan hệ 3NF (Third Normal Form) gồm 8 bảng dữ liệu:

1. **Categories (`Categories`)**: Quản lý 8 danh mục ngành hàng chuẩn Điện Máy Xanh:
   - Tivi, Tủ lạnh, Máy giặt, Máy lạnh - Điều hòa, Điện thoại, Laptop, Gia dụng, Máy lọc nước.
2. **Brands (`Brands`)**: Quản lý các thương hiệu chính hãng đối tác:
   - Samsung, Sony, LG, Panasonic, Toshiba, Aqua, TCL, Sunhouse, Karofi, Apple, Asus...
3. **Products (`Products`)**: Quản lý thiết bị điện máy chi tiết:
   - Mã SKU, tên sản phẩm, giá niêm yết gốc, giá bán khuyến mãi, thông số kỹ thuật (dạng `Tên: Giá trị`), chính sách trả góp 0%, bảo hành (tháng), số lượng tồn kho khả dụng (`StockQuantity`) và số lượng đã xuất bán (`SoldCount`).
4. **StockImports (`StockImports`)**: Nhật ký chứng từ nhập kho:
   - Mã phiếu nhập (`PNK...`), mã sản phẩm, số lượng nhập, đơn giá vốn, đối chiếu tồn trước và sau nhập (`StockBefore` -> `StockAfter`), nhà cung cấp, người lập phiếu, thời gian.
5. **Coupons (`Coupons`)**: Bảng mã giảm giá và voucher ưu đãi:
   - Mã Code, mô tả, loại giảm (`FixedAmount` hoặc `Percentage`), giá trị giảm, đơn hàng tối thiểu, mức giảm tối đa, giới hạn lượt dùng, số lượt đã sử dụng, thời hạn áp dụng, trạng thái kích hoạt.
6. **Orders (`Orders`) & OrderDetails (`OrderDetails`)**:
   - Quản lý giao dịch bán hàng, mã đơn hàng (`DMX...`), thông tin người nhận, địa chỉ hành chính chi tiết, mã voucher áp dụng, phương thức thanh toán (COD, Chuyển khoản VietQR, Trả góp 0%), trạng thái đơn hàng (Chờ xác nhận, Đang xử lý, Đang giao hàng, Hoàn thành, Đã hủy).
7. **ProductReviews (`ProductReviews`)**: Đánh giá và nhận xét chất lượng sản phẩm từ khách hàng (Rating từ 1 đến 5 sao).
8. **Users (`Users`)**: Quản lý tài khoản đăng nhập (Admin, Customer), số điện thoại 10 số, email, địa chỉ và trạng thái kích hoạt tài khoản (`IsActive`).

---

## 5. TÍNH NĂNG CHI TIẾT CỦA HỆ THỐNG

### Phân hệ Khách hàng (Storefront)
- **Trang chủ hiện đại**:
  - Banner quảng cáo trượt (Hero slider), thanh truy cập nhanh danh mục ngành hàng.
  - Khối **Khuyến mãi Giờ vàng (Flash Sale)** với đồng hồ đếm ngược thời gian thực.
  - Phân vùng sản phẩm nổi bật, sản phẩm bán chạy nhất kèm nhãn Trả góp 0% và Giảm sâu.
- **Thanh Widget Nổi Đa Năng (Floating Dock) & Trợ Lý Ảo Chatbot**:
  - **Nút Cuộn về đầu trang (Scroll to Top)**: Tự động hiển thị khi khách hàng cuộn chuột xuống quá 300px, click để lướt mượt mà lên đầu trang.
  - **Nút Gọi Hotline nhanh (`1800 1061`)**: Hiệu ứng vòng sóng rung nhẹ (pulse animation), hỗ trợ gọi trực tiếp tổng đài chăm sóc khách hàng miễn cước.
  - **Nút Yêu cầu gọi lại tư vấn (Callback Request Modal)**: Mở hộp thoại đăng ký nhận cuộc gọi tư vấn miễn phí trong 5 phút, xác thực số điện thoại 10 chữ số, ghi nhận nhu cầu thiết bị và phản hồi qua SweetAlert2.
  - **Chatbot Trợ lý ảo AI 24/7**:
    - Cung cấp trạng thái trực tuyến 24/7, lời chào thân thiện và danh sách gợi ý câu hỏi 1 chạm: *Tư vấn Smart Tivi 4K, Tủ lạnh Inverter, Máy giặt cửa ngang, Trả góp 0%, Tra cứu đơn hàng, Chính sách bảo hành, Voucher giảm giá*.
    - Bộ máy nhận diện từ khóa tự động phân tích câu hỏi người dùng, phản hồi thông số kỹ thuật tiêu biểu kèm nút điều hướng đến đúng danh mục mua sắm.
- **Tìm kiếm thông minh trực tiếp (Live Search / Autocomplete)**:
  - Nhập từ 2 ký tự vào thanh tìm kiếm tại Header, hệ thống tự động gọi API và hiển thị kết quả gợi ý sau 250ms (debounce).
  - Dropdown hiển thị ảnh thu nhỏ, tên SKU, ngành hàng, giá khuyến mãi, giá gốc và liên kết xem toàn bộ kết quả. Hỗ trợ phím Escape và click ngoài để đóng nhanh.
- **Bộ lọc đa tiêu chí (Faceted Filtering)**:
  - Lọc đồng thời theo Danh mục, Thương hiệu, 5 Khoảng giá (Dưới 5 triệu, 5-10 triệu, 10-15 triệu, 15-25 triệu, Trên 25 triệu).
  - Sắp xếp linh hoạt theo: Giá tăng dần, Giá giảm dần, Bán chạy nhất, Khuyến mãi nhiều nhất. Phân trang sản phẩm 12 mục/trang.
- **Trang Chi tiết Sản phẩm**:
  - Trưng bày ảnh lớn kết hợp thư viện ảnh phụ (Gallery).
  - Khối giá khuyến mãi, tính mức tiết kiệm và nhãn ưu đãi đi kèm.
  - Bảng thông số kỹ thuật chi tiết bóc tách từng thông số.
  - Kiểm soát tồn kho: Bộ chọn số lượng tự động giới hạn tối đa theo số lượng tồn kho khả dụng thực tế trong cơ sở dữ liệu (`max="@p.StockQuantity"`). Hiển thị trạng thái "Tạm hết hàng" khi tồn bằng 0.
  - Đánh giá sao & Nhận xét của khách mua hàng.
- **So sánh Sản phẩm (Product Comparison - Đặc thù Điện Máy)**:
  - Cho phép người dùng chọn tối đa 3 thiết bị để so sánh.
  - Thanh so sánh nổi ở đáy màn hình (Floating Compare Bar) hiển thị ảnh nhỏ, tên máy, nút xóa nhanh và nút chuyển đến trang so sánh.
  - Giao diện so sánh đối chiếu bảng thông số kỹ thuật từng dòng chi tiết, hỗ trợ modal thêm nhanh sản phẩm cùng ngành hàng.
- **Giỏ hàng & Áp dụng Voucher**:
  - Thêm, sửa số lượng, xóa sản phẩm khỏi giỏ hàng qua Session.
  - Hệ thống tự động kiểm tra số lượng tồn kho thực tế, ngăn chặn đặt mua vượt quá tồn khả dụng.
  - Hộp nhập Voucher liên kết động với cơ sở dữ liệu, tự động xác thực hạn sử dụng, lượt dùng và giá trị đơn hàng tối thiểu. Hỗ trợ danh sách thẻ gợi ý Voucher 1 chạm (`DMX500K`, `DMX100K`, `TRIAN5PT`).
- **Thanh toán (Checkout) & Mã VietQR Tự Động**:
  - Ràng buộc nhập số điện thoại người nhận đúng 10 số (đầu số 03, 05, 07, 08, 09), tự động lọc bỏ ký tự không phải số.
  - Tích hợp API đơn vị hành chính Việt Nam mới: Select Option chọn Tỉnh/Thành phố (34 đơn vị) tự động kích hoạt tải Phường/Xã tương ứng.
  - 3 phương thức thanh toán: COD, Chuyển khoản ngân hàng (VietQR), Trả góp 0%.
  - **Mã QR thanh toán VietQR động**:
    - Khi chọn hình thức Chuyển khoản ngân hàng, hệ thống sinh mã VietQR chuẩn Napas 247 tại màn hình Đặt hàng thành công (`OrderSuccess`).
    - Mã QR mã hóa đầy đủ thông tin: Ngân hàng MBBank, STK `0988123456`, Tên tài khoản `SIEU THI DIEN MAY XANH`, chính xác số tiền đơn hàng và nội dung chuyển khoản là Mã đơn hàng.
    - Cung cấp nút sao chép nhanh STK và Mã đơn hàng kèm thông báo SweetAlert2.
- **Theo dõi & Quản lý đơn hàng cá nhân**:
  - Tra cứu đơn hàng theo mã đơn hoặc số điện thoại mà không cần đăng nhập (`/Cart/TrackOrder`).
  - Tích hợp khung VietQR trực tiếp tại trang tra cứu đối với các đơn hàng chưa thanh toán.
  - Khách hàng đăng nhập có thể xem hồ sơ cá nhân, lịch sử đơn hàng và có quyền tự bấm "Hủy đơn hàng" khi đơn ở trạng thái "Chờ xác nhận" (hệ thống sẽ tự động hoàn trả số lượng vào kho hàng).
- **Đăng ký & Đăng nhập Bảo mật**:
  - Giới hạn trường số điện thoại 10 chữ số tại cả giao diện và DataAnnotations.
  - Bắt lỗi trùng lặp độc lập và trả về lỗi đúng ngay dưới từng ô nhập liệu: Tên đăng nhập đã tồn tại, Số điện thoại đã được đăng ký, Email đã sử dụng.
  - Xử lý đăng nhập thông minh: Kiểm tra tài khoản không tồn tại, tài khoản bị tạm khóa (`IsActive == false`), và mật khẩu không trùng khớp.

---

### Phân hệ Quản trị (Admin CMS)
- **Trung tâm Điều hành (Dashboard)**:
  - 4 Thẻ KPI chiến lược: Tổng doanh thu thực tế, Tổng đơn hàng, Khách hàng thành viên, Thiết bị đang kinh doanh.
  - Biểu đồ phân tích kinh doanh, danh sách đơn hàng mới cần xử lý, danh sách sản phẩm bán chạy nhất và cảnh báo thiết bị sắp hết hàng.
- **Quản lý Sản phẩm (Products CMS)**:
  - Thêm mới, chỉnh sửa thông tin, ảnh, thông số kỹ thuật, bảo hành, trạng thái mở bán.
  - **Khóa sửa tay số lượng tồn kho**: Trường tồn kho trong form sản phẩm được thiết lập chế độ Chỉ đọc (Read-only) kèm nhãn `Khóa sửa tay`. Khi tạo sản phẩm mới, số lượng mặc định bằng 0 và hệ thống tự chuyển hướng sang lập phiếu nhập kho.
  - Xóa an toàn: Tự động chuyển sản phẩm sang trạng thái "Ngừng kinh doanh" nếu sản phẩm đó đã từng phát sinh đơn hàng trong quá khứ.
- **Quản lý Kho hàng & Nhập kho khép kín (Warehouse & Inventory)**:
  - Quy tắc kho: Tồn kho chỉ tăng khi lập Phiếu nhập kho và tự động giảm khi phát sinh đơn bán hàng.
  - 4 Thẻ KPI kho: Tổng tồn kho khả dụng, Tổng giá trị hàng tồn (tính theo giá vốn/khuyến mãi), Cảnh báo hàng sắp hết ($\le 10$) hoặc hết hàng ($= 0$), Tổng số chứng từ đã lập.
  - Form lập phiếu nhập kho sticky với tính năng xem trước biến động tồn kho tự động: `Tồn hiện tại` -> `+Nhập thêm` -> `Tồn sau nhập`.
  - Bộ lọc nhanh kiểm kê theo 4 trạng thái: Tất cả mặt hàng, Hết hàng, Sắp hết, Đủ hàng kèm nút "Nhập kho" 1-click.
  - Nhật ký 30 phiếu nhập kho gần nhất phục vụ kiểm toán nội bộ.
  - Tự động hoàn lại số lượng tồn kho khi đơn hàng bị hủy bởi khách hàng hoặc quản trị viên.
- **In Hóa đơn Bán lẻ kiêm Phiếu xuất kho A4**:
  - Tại trang chi tiết đơn hàng, quản trị viên có thể bấm **In hóa đơn & Phiếu xuất kho**.
  - Kết xuất mẫu biểu A4 chuẩn mực: Thông tin đơn vị bán, logo Điện Máy Xanh, mã chứng từ, thông tin khách hàng, bảng chi tiết sản phẩm, tổng tiền bằng số và tự động chuyển đổi thành số tiền bằng chữ tiếng Việt, kèm 3 chữ ký (Người mua, Giao hàng, Thủ kho/Kế toán).
- **Báo cáo Thống kê Doanh thu & Hiệu quả Kinh doanh (Revenue Reports)**:
  - Bộ lọc thời gian: Hôm nay, 7 ngày, 30 ngày, Tháng này, Tháng trước, Năm nay hoặc khoảng ngày tùy chọn.
  - Lọc theo ngành hàng và trạng thái đơn hàng.
  - 5 Thẻ chỉ số tài chính: Doanh thu thuần, Lợi nhuận gộp ước tính, Tỷ suất lợi nhuận (%), Số đơn hàng, Số lượng thiết bị bán, Giá trị trung bình đơn (AOV).
  - 2 Biểu đồ Chart.js: Biểu đồ cột kép doanh thu/lợi nhuận theo ngày và biểu đồ tròn cơ cấu ngành hàng.
  - Bảng Top 10 sản phẩm đóng góp doanh thu lớn nhất và bảng chi tiết doanh thu từng ngày.
- **Xuất Báo cáo Dạng PDF (Export PDF)**:
  - Tích hợp nút **Tải file PDF ngay** (qua thư viện `html2pdf.js`), tạo tệp `.pdf` lưu vào máy tính chỉ sau 1 lần bấm.
  - Cung cấp nút in ấn và lưu PDF bằng lệnh in trình duyệt (Ctrl + P) chuẩn khổ A4 dọc.
- **Quản lý Thương hiệu hãng (Brands CMS)**:
  - Thêm, sửa, xem trước logo hãng, xuất xứ quốc gia, theo dõi số lượng thiết bị trực thuộc từng hãng.
- **Quản lý Mã giảm giá (Coupons CMS)**:
  - Cấu hình mã voucher, loại giảm giá (tiền mặt / %), đơn tối thiểu, mức giảm tối đa, hạn mức số lượt dùng, thời hạn, bật/tắt kích hoạt mã.
- **Quản lý Danh mục, Người dùng, Đánh giá**:
  - Quản lý danh mục ngành hàng, sắp xếp thứ tự hiển thị.
  - Quản lý danh sách người dùng, kích hoạt hoặc khóa tài khoản khách hàng.
  - Kiểm duyệt và phê duyệt hoặc gỡ bỏ đánh giá sản phẩm.

---

## 6. HƯỚNG DẪN CÀI ĐẶT VÀ CHẠY DỰ ÁN

### Yêu cầu phần mềm
- [.NET SDK 8.0](https://dotnet.microsoft.com/download/dotnet/8.0) trở lên.
- Microsoft SQL Server 2019/2022 hoặc SQL Server Express (hoặc LocalDB).
- Visual Studio 2022 (khuyến nghị có khối lượng công việc *ASP.NET and web development*) hoặc Visual Studio Code.

### Các bước triển khai

#### Bước 1: Sao chép mã nguồn và cấu hình chuỗi kết nối
Mở tệp `appsettings.json` tại thư mục gốc của dự án và kiểm tra chuỗi kết nối SQL Server:
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=.\\SQLEXPRESS;Database=WebsiteDienMayDb;Trusted_Connection=True;TrustServerCertificate=True;MultipleActiveResultSets=true"
  }
}
```
*(Nếu bạn sử dụng phiên bản SQL Server mặc định hoặc tài khoản SQL Authentication, hãy điều chỉnh `Server` hoặc thêm `User Id` và `Password` tương ứng).*

#### Bước 2: Cập nhật Cơ sở dữ liệu qua EF Core
Mở Terminal hoặc Command Prompt tại thư mục dự án và thực hiện lệnh:
```bash
dotnet ef database update
```
Lệnh này sẽ tự động chạy toàn bộ các bản Migration:
1. `20261106071258_InitialCreate.cs`: Tạo cấu trúc 7 bảng dữ liệu ban đầu.
2. `20261107120936_AddWarehouseInventory.cs`: Tạo bảng quản lý phiếu nhập kho `StockImports`.
3. `20261107125842_AddCouponsTable.cs`: Tạo bảng quản lý mã giảm giá `Coupons`.

#### Bước 3: Biên dịch và Khởi chạy Ứng dụng
Chạy ứng dụng bằng lệnh:
```bash
dotnet run
```
Hoặc nhấn `F5` / `Ctrl + F5` trong Visual Studio 2022.

Ứng dụng sẽ tự động kích hoạt bộ khởi tạo `DbInitializer` khi khởi động lần đầu, nạp đầy đủ dữ liệu mẫu gồm 8 danh mục, 32 sản phẩm, thương hiệu, tài khoản, chứng từ kho và các mã voucher hoạt động sẵn.

Truy cập website tại địa chỉ:
- Giao diện Cửa hàng: `http://localhost:5245` (hoặc cổng HTTPS `https://localhost:7257`).
- Giao diện Quản trị: `http://localhost:5245/Admin`

---

## 7. TÀI KHOẢN ĐĂNG NHẬP MẶC ĐỊNH

Hệ thống đã nạp sẵn các tài khoản thử nghiệm:

| Vai trò (Role) | Tên đăng nhập | Mật khẩu | Số điện thoại | Quyền hạn và phạm vi sử dụng |
| :--- | :--- | :--- | :--- | :--- |
| **Quản trị viên (Admin)** | `admin` | `123456` | `0901234567` | Toàn quyền quản trị hệ thống, quản lý sản phẩm, kho hàng, duyệt đơn, in hóa đơn, xuất báo cáo PDF |
| **Khách hàng mẫu 1** | `giangnt` | `123456` | `0988111222` | Mua sắm, thanh toán VietQR, theo dõi lịch sử đơn hàng, gửi đánh giá nhận xét sản phẩm |
| **Khách hàng mẫu 2** | `minhtuan` | `123456` | `0977333444` | Mua sắm, áp dụng mã ưu đãi, thanh toán trực tuyến |

*(Lưu ý: Màn hình Đăng nhập được trang bị sẵn 2 nút bấm tự động điền nhanh tài khoản Admin và Khách hàng để kiểm thử nhanh chóng).*

---

## 8. DANH SÁCH MÃ GIẢM GIÁ (VOUCHER) KHẢ DỤNG

Các mã giảm giá đã được thiết lập sẵn trong cơ sở dữ liệu để thử nghiệm tính năng giỏ hàng:

| Mã Voucher | Loại giảm | Mức giảm | Điều kiện áp dụng |
| :--- | :--- | :--- | :--- |
| `DMX100K` | Tiền mặt | 100.000 VNĐ | Đơn hàng từ 1.000.000 VNĐ |
| `DMX500K` | Tiền mặt | 500.000 VNĐ | Đơn hàng từ 5.000.000 VNĐ |
| `GIAM1TRIEU` | Tiền mặt | 1.000.000 VNĐ | Đơn hàng từ 15.000.000 VNĐ (Tivi, Tủ lạnh cao cấp) |
| `TRIAN5PT` | Phần trăm | 5% (Tối đa 1.500.000 VNĐ) | Đơn hàng từ 3.000.000 VNĐ |
| `DIENMAYXANH` | Tiền mặt | 200.000 VNĐ | Đơn hàng từ 2.000.000 VNĐ |

---

## 9. QUY TẮC PHÁT TRIỂN VÀ TIÊU CHUẨN THIẾT KẾ

1. **Tuân thủ tiêu chuẩn hình ảnh**: Toàn bộ hình ảnh sản phẩm và biểu tượng ngành hàng sử dụng nguồn trực tiếp từ CDN chính thức của `dienmayxanh.com` (`cdn.tgdd.vn` và `cdnv2.tgdd.vn`), không sử dụng ảnh vẽ minh họa hoặc vector tự sinh.
2. **Nguyên tắc giao diện chuẩn mực**: Toàn bộ hệ thống không sử dụng biểu tượng cảm xúc (zero emojis) nhằm đảm bảo phong cách làm việc chuyên nghiệp của một sàn thương mại điện tử thực tế.
3. **Responsive Design**: Tối ưu hóa hiển thị trên mọi thiết bị: Desktop màn hình rộng, Tablet và điện thoại thông minh (Mobile).

---

*(Bản quyền mã nguồn dự án thuộc về nhóm phát triển hệ thống Website Thương Mại Điện Tử Điện Máy).*
