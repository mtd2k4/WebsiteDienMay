# HỆ THỐNG WEBSITE THƯƠNG MẠI ĐIỆN TỬ ĐIỆN MÁY XANH

Hệ thống website thương mại điện tử chuyên ngành thiết bị điện tử, điện gia dụng và công nghệ số được xây dựng trên nền tảng **ASP.NET Core 8.0 MVC** kết hợp **Entity Framework Core** và cơ sở dữ liệu **Microsoft SQL Server**, mô phỏng theo mô hình trải nghiệm và quy trình vận hành thực tế của chuỗi siêu thị bán lẻ Điện Máy Xanh.

---

## 1. TỔNG QUAN HỆ THỐNG

Dự án cung cấp một giải pháp phần mềm thương mại điện tử toàn diện bao gồm hai phân hệ độc lập:
1. **Phân hệ Cửa hàng Bán lẻ (Storefront UX)**: Phục vụ khách hàng tìm kiếm trực tiếp, chọn phiên bản biến thể (Dung lượng/Màu sắc/Kích cỡ/Công suất) tự động đổi ảnh màu tương ứng, so sánh thông số kỹ thuật, tương tác trợ lý ảo AI chatbot, áp dụng voucher ưu đãi, tính thuế GTGT (VAT 8%) kèm xuất hóa đơn điện tử công ty, tính toán các gói trả góp 0% lãi suất, thanh toán quét mã VietQR tự động và tra cứu tiến độ đơn hàng.
2. **Phân hệ Quản trị Vận hành (Admin CMS)**: Cung cấp trung tâm điều hành tập trung cho nhà bán hàng để kiểm soát danh mục sản phẩm và biến thể màu sắc, quản lý kho hàng khép kín (nhập/xuất tự động, khóa sửa tay tồn kho, xuất phiếu nhập kho PDF), quản lý banner quảng cáo động, cấu hình tài khoản ngân hàng thụ hưởng & mẫu VietQR, xử lý đơn hàng, quản lý mã giảm giá, quản lý người dùng (phân quyền, khóa/mở khóa, xóa tài khoản), in hóa đơn GTGT chuẩn A4 và xuất báo cáo tài chính định dạng PDF chuyên sâu.

---

## 2. CÔNG NGHỆ VÀ THƯ VIỆN SỬ DỤNG

### Backend & Cơ sở dữ liệu
- **Framework**: [.NET 8.0 (C# 12)](https://dotnet.microsoft.com/) - ASP.NET Core MVC.
- **ORM**: [Entity Framework Core 8.0.11](https://docs.microsoft.com/ef/core/) (Code-First với hệ thống Migration quản lý phiên bản cơ sở dữ liệu tự động).
- **Cơ sở dữ liệu**: Microsoft SQL Server (Hỗ trợ SQL Server 2019/2022, SQL Server Express, LocalDB).
- **Kiến trúc**: MVC (Model - View - Controller), Repository pattern thông qua `ApplicationDbContext`, JSON Configuration Store (`BankConfigStore`) cho cấu hình ngân hàng VietQR, Session-based Cart & Authentication.
- **Bảo mật & Ràng buộc dữ liệu**: AntiForgeryToken, DataAnnotations (Regex 10 chữ số chuẩn đầu số viễn thông Việt Nam `^0[35789][0-9]{8}$`), kiểm tra chống trùng lặp tài khoản độc lập cho Username, Email và Số điện thoại.

### Frontend & Giao diện người dùng
- **View Engine**: Razor View Engine (`.cshtml`) với cơ chế phân tách Layout độc lập:
  - `_Layout.cshtml`: Giao diện khách hàng tối ưu theo phong cách Điện Máy Xanh (Tone màu Xanh dương `#0071e3` và Vàng `#fed700`), tích hợp thanh Floating Dock, Chatbot AI và Modal yêu cầu tư vấn.
  - `_AdminLayout.cshtml`: Giao diện quản trị hiện đại (Dark Navy Sidebar `#0d1726` kết hợp Card content sáng màu).
  - `_AuthLayout.cshtml`: Giao diện tinh gọn dành riêng cho màn hình Đăng nhập / Đăng ký.
- **Framework CSS**: Bootstrap 5.3 + Bộ biểu tượng Bootstrap Icons (1.11.3).
- **Thư viện tương tác & Tiện ích mở rộng**:
  - `SweetAlert2 (v11)`: Hộp thoại thông báo Toast tự tắt và xác nhận thao tác an toàn (`.swal-confirm-form`).
  - `Chart.js`: Biểu đồ cột kép và biểu đồ tròn phân tích doanh thu theo ngày và cơ cấu ngành hàng.
  - `html2pdf.js (0.10.1)`: Kết xuất báo cáo doanh thu, hóa đơn bán hàng GTGT và phiếu nhập kho thành tệp tin PDF chuẩn A4 trực tiếp trên trình duyệt.
- **Tích hợp Cổng thanh toán & Dịch vụ số**:
  - `VietQR Dynamic API (img.vietqr.io)`: Tạo mã QR thanh toán động chuẩn Napas 247, đồng bộ trực tiếp từ mục **Cấu hình ngân hàng** trong Admin (Mã ngân hàng BIN/VietQR, Số tài khoản, Chủ tài khoản, Mẫu QR, Số tiền và Nội dung chuyển khoản theo mã đơn).
  - `API Đơn vị Hành chính Mới (provinces.open-api.vn)`: Cập nhật 34 Tỉnh/Thành phố và Phường/Xã với cơ chế Backend Proxy tránh lỗi CORS.

---

## 3. CẤU TRÚC THƯ MỤC DỰ ÁN

```text
WebsiteDienMay/
├── Controllers/                    # Bộ điều hướng xử lý nghiệp vụ
│   ├── AccountController.cs       # Xác thực, đăng ký, đăng nhập, kiểm tra trùng lặp, hồ sơ, hủy đơn
│   ├── AdminController.cs         # Trung tâm điều hành CMS, kho hàng, banner, ngân hàng VietQR, báo cáo PDF
│   ├── CartController.cs          # Giỏ hàng biến thể, tồn kho, voucher, thuế VAT 8%, trả góp 0%, VietQR
│   ├── HomeController.cs          # Trang chủ (Banner động), giới thiệu, chính sách bảo hành
│   └── ProductController.cs       # Danh sách, chi tiết (đổi ảnh theo biến thể màu), tìm kiếm, so sánh
├── Data/                          # Cơ sở dữ liệu, cấu hình ngân hàng và khởi tạo dữ liệu mẫu
│   ├── ApplicationDbContext.cs   # Cấu hình DbSet, khóa ngoại và ràng buộc dữ liệu
│   ├── BankConfigStore.cs        # Lưu trữ và tạo URL mã VietQR động từ cấu hình ngân hàng Admin
│   ├── bank_configs.json         # Dữ liệu cấu hình các tài khoản ngân hàng thụ hưởng
│   └── DbInitializer.cs          # Tự động Migration và nạp dữ liệu mẫu (biến thể màu, banner, thuế VAT)
├── Migrations/                    # Lịch sử các bản di chuyển cơ sở dữ liệu EF Core
│   ├── 20261106071258_InitialCreate.cs
│   ├── 20261107120936_AddWarehouseInventory.cs
│   ├── 20261107125842_AddCouponsTable.cs
│   └── 20261108155757_AddVariantsBannersVatInstallment.cs
├── Models/                        # Định nghĩa thực thể và mô hình dữ liệu
│   ├── Entities.cs               # Category, Brand, Product, StockImport, Order, Coupon, Banner, BankAccountConfig...
│   └── ViewModels.cs             # ProductVariantItem, ProductList, Cart, Checkout, Warehouse, RevenueReport...
├── Views/                         # Giao diện người dùng Razor View
│   ├── Account/                  # Login, Register, Profile, AccessDenied
│   ├── Admin/                    # Index, Products, ProductForm, Warehouse, PrintStockImport, Orders, PrintInvoice,
│   │                             # Banners, BankConfig, Brands, Categories, Coupons, Users, Reviews, RevenueReport...
│   ├── Cart/                     # Index, Checkout (VAT 8% + Trả góp 0%), OrderSuccess (VietQR), TrackOrder
│   ├── Home/                     # Index, About, WarrantyPolicy, Privacy
│   ├── Product/                  # Index, Detail (Chọn biến thể đổi ảnh màu), Compare
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

Hệ thống bao gồm 9 bảng dữ liệu SQL Server chuẩn 3NF kết hợp kho cấu hình ngân hàng VietQR:

1. **Categories (`Categories`)**: Quản lý 8 danh mục ngành hàng chuẩn Điện Máy Xanh:
   - Tivi, Tủ lạnh, Máy giặt, Máy lạnh - Điều hòa, Điện thoại, Laptop, Gia dụng, Máy lọc nước.
2. **Brands (`Brands`)**: Quản lý các thương hiệu chính hãng đối tác:
   - Samsung, Sony, LG, Panasonic, Toshiba, Aqua, TCL,Hitachi, Casper, Apple, OPPO, Xiaomi, Asus, Dell, HP, Lenovo...
3. **Products (`Products`)**: Quản lý thiết bị điện máy & biến thể kèm ảnh màu sắc:
   - Mã SKU, tên sản phẩm, giá niêm yết gốc, giá bán khuyến mãi, danh sách **Biến thể sản phẩm (`ProductVariants`)** theo cú pháp 4 cột `Tên biến thể | Giá bán | Giá gốc | URL ảnh màu biến thể`, thông số kỹ thuật, chính sách trả góp 0%, bảo hành (tháng), số lượng tồn kho khả dụng (`StockQuantity`) và số lượng đã xuất bán (`SoldCount`).
4. **StockImports (`StockImports`)**: Nhật ký chứng từ nhập kho:
   - Mã phiếu nhập (`PNK...`), mã sản phẩm, số lượng nhập, đơn giá vốn, đối chiếu tồn trước và sau nhập (`StockBefore` -> `StockAfter`), nhà cung cấp, người lập phiếu, thời gian, hỗ trợ xuất **Phiếu nhập kho kiêm Hóa đơn nhập hàng dạng PDF chuẩn A4**.
5. **Coupons (`Coupons`)**: Bảng mã giảm giá và voucher ưu đãi:
   - Mã Code, mô tả, loại giảm (`FixedAmount` hoặc `Percentage`), giá trị giảm, đơn hàng tối thiểu, mức giảm tối đa, giới hạn lượt dùng, số lượt đã sử dụng, thời hạn áp dụng, trạng thái kích hoạt.
6. **Banners (`Banners`)**: Quản lý hệ thống Banner quảng cáo động trên Trang chủ:
   - Tiêu đề chiến dịch, thông điệp phụ, đường dẫn ảnh (`ImageUrl`), liên kết đích (`LinkUrl`), vị trí hiển thị (`Hero` hoặc `SubBanner`), thứ tự ưu tiên (`DisplayOrder`) và trạng thái bật/tắt (`IsActive`).
7. **Orders (`Orders`) & OrderDetails (`OrderDetails`)**:
   - Quản lý giao dịch bán hàng, mã đơn hàng (`DMX...`), phiên bản biến thể và ảnh màu đã chọn (`SelectedVariant`, `ProductImage`), thuế GTGT (`VatAmount` 8%), thông tin xuất hóa đơn đỏ doanh nghiệp (`RequireVatInvoice`, `CompanyName`, `CompanyTaxCode`, `CompanyAddress`, `CompanyEmail`), gói trả góp chi tiết (`InstallmentInfo`), phương thức thanh toán (COD, Chuyển khoản VietQR, Trả góp 0%).
8. **ProductReviews (`ProductReviews`)**: Đánh giá và nhận xét chất lượng sản phẩm từ khách hàng (Rating từ 1 đến 5 sao).
9. **Users (`Users`)**: Quản lý tài khoản đăng nhập (Admin, Customer), số điện thoại 10 số, email, địa chỉ và trạng thái kích hoạt tài khoản (`IsActive`).
10. **BankAccountConfig (`BankConfigStore` / `bank_configs.json`)**: Lưu trữ cấu hình các tài khoản ngân hàng thụ hưởng của siêu thị (Mã ngân hàng VietQR, Tên ngân hàng, Số tài khoản, Chủ tài khoản, Chi nhánh, Mẫu giao diện QR `compact2`/`compact`/`print`/`qr_only`, tài khoản mặc định và trạng thái hoạt động).

---

## 5. TÍNH NĂNG CHI TIẾT CỦA HỆ THỐNG

### Phân hệ Khách hàng (Storefront)
- **Trang chủ hiện đại & Banner Quảng cáo Động**:
  - Hệ thống Hero Banner trượt tự động và Sub-Banner đôi khuyến mãi được tải động từ cơ sở dữ liệu (quản trị viên có thể thay đổi trực tiếp trong Admin).
  - Khối **Khuyến mãi Giờ vàng (Flash Sale)** với đồng hồ đếm ngược thời gian thực.
  - Phân vùng sản phẩm nổi bật, sản phẩm theo từng ngành hàng kèm nhãn Trả góp 0% và Giảm sâu.
- **Trang Chi tiết Sản phẩm & Chọn Biến Thể Đổi Ảnh Màu Tương Ứng**:
  - Hỗ trợ đầy đủ các biến thể theo ngành hàng: **Tủ lạnh** (*Màu Đen Ánh Kim, Màu Bạc Inox, Màu Xám Khói*), **Điện thoại** (*Dung lượng 128GB/256GB/512GB/1TB & Màu sắc*), **Laptop** (*Cấu hình RAM/SSD & Màu vỏ*), **Tivi** (*Kích cỡ 55/65/75 inch*), **Máy lạnh** (*Công suất 1 HP / 1.5 HP / 2 HP*), **Máy giặt** (*Khối lượng giặt & Màu sắc*).
  - Mỗi nút chọn biến thể hiển thị kèm ảnh thumbnail thu nhỏ của màu sắc/phiên bản đó.
  - **Tự động chuyển đổi ảnh theo màu**: Khi khách hàng bấm chọn một biến thể (ví dụ chọn *Màu Đen Ánh Kim* hoặc *Màu Bạc Inox* của Tủ lạnh), ảnh lớn chính (`#mainProductImage`) lập tức chuyển sang đúng ảnh màu tương ứng, cập nhật nhãn phiên bản đang xem, đánh dấu sáng ảnh tương ứng trên thanh Gallery và truyền đúng ảnh biến thể đó vào Giỏ hàng.
  - Kiểm soát tồn kho: Bộ chọn số lượng tự động giới hạn tối đa theo số lượng tồn kho khả dụng thực tế (`max="@p.StockQuantity"`).
- **Giỏ hàng, Thuế GTGT (VAT 8%) & Áp dụng Voucher**:
  - Hiển thị rõ phiên bản/màu sắc biến thể và ảnh tương ứng của từng mặt hàng trong giỏ.
  - Tự động tính **Thuế GTGT (VAT 8%)** trên tổng tiền hàng sau chiết khấu voucher: `Tổng thanh toán = (Tạm tính - Giảm giá Voucher) + Thuế VAT (8%)`.
  - Hộp nhập Voucher liên kết động với cơ sở dữ liệu kèm danh sách thẻ gợi ý Voucher 1 chạm (`DMX500K`, `DMX100K`, `GIAM1TRIEU`, `TRIAN5PT`).
- **Thanh toán (Checkout): Xuất Hóa Đơn Đỏ Công Ty, Bộ Tính Trả Góp 0% & Mã VietQR**:
  - **Tùy chọn Xuất hóa đơn điện tử GTGT (VAT 8%) cho Công ty / Tổ chức**: Khi tích chọn, biểu mẫu mở rộng cho phép nhập *Tên công ty trên GPKD, Mã số thuế (MST), Địa chỉ trụ sở, Email nhận hóa đơn điện tử* và in trực tiếp lên Hóa đơn GTGT.
  - **Cấu hình các phương thức Trả góp 0% lãi suất trực quan**:
    - Khi chọn phương thức *Đăng ký Trả góp 0% lãi suất*, khách hàng được chọn giữa 2 hình thức:
      1. **Trả góp qua Thẻ tín dụng** (Vietcombank, Techcombank, MBBank, VPBank, ACB, Sacombank, BIDV, VIB - Visa/Mastercard/JCB).
      2. **Trả góp qua Công ty Tài chính** (Home Credit, FE Credit, HD Saison, Mirae Asset - duyệt hồ sơ bằng CCCD gắn chip).
    - Tùy chọn **Kỳ hạn trả góp** (3, 6, 9, 12 tháng) và **Tỷ lệ trả trước** (0%, 20%, 30%, 50%).
    - Bảng tính tự động hiển thị trực tiếp: *Số tiền trả trước*, *Dư nợ trả góp* và *Số tiền góp mỗi tháng*.
  - **Mã QR thanh toán VietQR động đồng bộ từ Cấu hình Ngân hàng Admin**:
    - Khi chọn *Chuyển khoản ngân hàng / Quét mã VietQR 24/7*, màn hình Đặt hàng thành công (`OrderSuccess`) và Tra cứu đơn hàng (`TrackOrder`) tự động sinh mã VietQR chuẩn Napas 247 từ tài khoản ngân hàng mặc định trong Admin.
    - Nếu siêu thị kích hoạt nhiều tài khoản ngân hàng thụ hưởng (ví dụ MBBank, Vietcombank, Techcombank), khách hàng có thể bấm chuyển đổi giữa các ngân hàng ngay trên màn hình thanh toán để quét mã QR thuận tiện nhất.
- **Thanh Widget Nổi Đa Năng (Floating Dock) & Trợ Lý Ảo Chatbot 24/7**:
  - Nút Cuộn về đầu trang (Scroll to Top), nút Gọi Hotline nhanh (`1800 1061`), Modal đăng ký nhận cuộc gọi tư vấn miễn phí trong 5 phút và Chatbot AI 24/7 gợi ý sản phẩm theo nhu cầu.
- **Tìm kiếm thông minh trực tiếp (Live Search) & So sánh sản phẩm (Compare)**:
  - Autocomplete gợi ý sản phẩm kèm ảnh và giá sau 250ms; thanh so sánh nổi cho phép đối chiếu bảng thông số kỹ thuật của tối đa 3 thiết bị cùng lúc.
- **Đăng ký & Đăng nhập Bảo mật**:
  - Giới hạn trường số điện thoại đúng 10 chữ số, bắt lỗi trùng lặp độc lập cho Tên đăng nhập, Số điện thoại và Email.

---

### Phân hệ Quản trị (Admin CMS)
- **Trung tâm Điều hành (Dashboard)**:
  - 4 Thẻ KPI chiến lược (Tổng doanh thu thực tế, Tổng đơn hàng, Khách hàng thành viên, Thiết bị đang kinh doanh), biểu đồ kinh doanh, đơn hàng mới và cảnh báo sắp hết hàng.
- **Quản lý Sản phẩm & Trình tạo Biến thể kèm Ảnh màu (`/Admin/Products`, `/Admin/CreateProduct`, `/Admin/EditProduct`)**:
  - Thêm mới, chỉnh sửa thông tin, ảnh đại diện, bộ sưu tập ảnh phụ, thông số kỹ thuật, bảo hành.
  - **Trình tạo biến thể trực quan kèm ảnh màu**: Hỗ trợ nhập nhanh *Tên phiên bản/Màu sắc*, *Giá bán KM*, *Giá niêm yết gốc* và *URL ảnh màu tương ứng*. Hiển thị danh sách thẻ biến thể trực quan có ảnh thu nhỏ, cho phép sửa nhanh URL ảnh riêng của từng màu sắc hoặc xóa từng dòng biến thể.
  - **Khóa sửa tay số lượng tồn kho**: Trường tồn kho ở chế độ Chỉ đọc (`Khóa sửa tay`), đảm bảo tồn kho chỉ biến động qua nghiệp vụ Nhập kho và Bán hàng.
- **Quản lý Kho hàng & Xuất Phiếu Nhập Kho PDF (`/Admin/Warehouse`, `/Admin/PrintStockImport/{id}`)**:
  - Lập phiếu nhập kho kiểm soát tồn trước và sau nhập (`StockBefore` -> `StockAfter`).
  - Mỗi dòng lịch sử nhập kho và mỗi phiếu nhập vừa tạo đều có nút **Xuất PDF / In phiếu** mở ra mẫu **Phiếu Nhập Kho Kiêm Hóa Đơn Nhập Hàng chuẩn A4** (đầy đủ thông tin nhà cung cấp, bảng kê SKU, đơn giá nhập, thành tiền bằng số & bằng chữ tiếng Việt, chữ ký Người lập phiếu - Giao nhận - Thủ kho - Kế toán trưởng) tích hợp nút tải tệp `.pdf` chỉ với 1 lần bấm (`html2pdf.js`).
- **Quản lý Banner Quảng cáo Trang chủ (`/Admin/Banners`)**:
  - Thêm mới, chỉnh sửa, bật/tắt hiển thị, sắp xếp thứ tự ưu tiên hoặc xóa các Banner lớn đầu trang (`Hero`) và Banner phụ khuyến mãi (`SubBanner`) kèm khung xem trước hình ảnh trực tiếp.
- **Cấu hình Ngân hàng & Thanh toán VietQR (`/Admin/BankConfig`)**:
  - Quản lý danh sách các tài khoản ngân hàng thụ hưởng của siêu thị (hỗ trợ chọn nhanh 14 ngân hàng nội địa chuẩn Napas 247: MB, VCB, TCB, VietinBank, BIDV, ACB, VPBank, TPBank, Sacombank, VIB, MSB, HDBank, SHB, OCB).
  - Tùy chỉnh Số tài khoản, Chủ tài khoản, Chi nhánh, Mẫu giao diện QR (`compact2`, `compact`, `print`, `qr_only`) kèm **Khung xem trước mã VietQR trực tiếp** ngay khi gõ.
  - Hỗ trợ đặt tài khoản làm **Mặc định VietQR**, bật/tắt trạng thái hoạt động hoặc xóa tài khoản ngân hàng.
- **Quản lý Đơn đặt hàng & In Hóa đơn GTGT A4 (`/Admin/Orders`, `/Admin/PrintInvoice/{id}`)**:
  - Duyệt trạng thái đơn hàng, cập nhật trạng thái thanh toán, xem chi tiết biến thể sản phẩm, gói trả góp 0% và thông tin xuất hóa đơn công ty.
  - Kết xuất **Hóa đơn Giá trị gia tăng (VAT 8%) kiêm Phiếu xuất kho chuẩn A4** (hỗ trợ cả tải file `.pdf` trực tiếp và lệnh in trình duyệt).
- **Báo cáo Thống kê Doanh thu & Xuất Báo cáo PDF (`/Admin/RevenueReport`, `/Admin/ExportRevenuePdf`)**:
  - Phân tích doanh thu thuần, lợi nhuận gộp, tỷ suất lợi nhuận, biểu đồ Chart.js theo ngày và cơ cấu ngành hàng, xuất báo cáo tổng hợp ra tệp PDF chuẩn A4.
- **Quản lý Người dùng (`/Admin/Users`)**:
  - Tìm kiếm người dùng, chuyển đổi vai trò (`Admin` / `Customer`), khóa hoặc mở khóa tài khoản (`ToggleUserStatus`).
  - **Xóa tài khoản người dùng (`DeleteUser`)**: Tích hợp nút xóa tài khoản kèm hộp thoại xác nhận SweetAlert2, tự động bảo vệ tài khoản Quản trị viên gốc (`admin`) và tài khoản đang đăng nhập, đồng thời gỡ liên kết khóa ngoại an toàn để bảo toàn lịch sử doanh thu đơn hàng.
- **Quản lý Thương hiệu (`/Admin/Brands`), Danh mục (`/Admin/Categories`), Mã giảm giá (`/Admin/Coupons`), Đánh giá (`/Admin/Reviews`)**.

---

## 6. HƯỚNG DẪN CÀI ĐẶT VÀ CHẠY DỰ ÁN

### Yêu cầu phần mềm
- [.NET SDK 8.0](https://dotnet.microsoft.com/download/dotnet/8.0) trở lên.
- Microsoft SQL Server 2019/2022 hoặc SQL Server Express (hoặc LocalDB).
- Visual Studio 2022 hoặc Visual Studio Code.

### Các bước triển khai

#### Bước 1: Kiểm tra chuỗi kết nối SQL Server
Mở tệp `appsettings.json` tại thư mục gốc của dự án:
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=.\\SQLEXPRESS;Database=WebsiteDienMayDb;Trusted_Connection=True;TrustServerCertificate=True;MultipleActiveResultSets=true"
  }
}
```

#### Bước 2: Cập nhật Cơ sở dữ liệu qua EF Core
Mở Terminal tại thư mục dự án và thực hiện lệnh:
```bash
dotnet ef database update
```
Lệnh này sẽ áp dụng toàn bộ 4 bản Migration:
1. `20261106071258_InitialCreate.cs`: Khởi tạo cấu trúc các bảng dữ liệu cốt lõi.
2. `20261107120936_AddWarehouseInventory.cs`: Bổ sung bảng quản lý phiếu nhập kho `StockImports`.
3. `20261107125842_AddCouponsTable.cs`: Bổ sung bảng quản lý mã giảm giá `Coupons`.
4. `20261108155757_AddVariantsBannersVatInstallment.cs`: Bổ sung bảng `Banners`, trường biến thể sản phẩm `ProductVariants`, biến thể chi tiết đơn hàng `SelectedVariant`, thuế GTGT `VatAmount`, thông tin hóa đơn doanh nghiệp và gói trả góp `InstallmentInfo`.

#### Bước 3: Biên dịch và Khởi chạy Ứng dụng
Chạy ứng dụng bằng lệnh:
```bash
dotnet run
```
Ứng dụng sẽ tự động kích hoạt `DbInitializer` và `BankConfigStore` khi khởi động, nạp đầy đủ dữ liệu mẫu gồm 8 danh mục, 32 sản phẩm kèm biến thể và ảnh màu tương ứng, thương hiệu, banner trang chủ, cấu hình ngân hàng VietQR, chứng từ kho và các mã voucher hoạt động sẵn.

Truy cập website tại địa chỉ:
- Giao diện Cửa hàng: `http://localhost:5245` (hoặc cổng HTTPS `https://localhost:7257`).
- Giao diện Quản trị: `http://localhost:5245/Admin`

---

## 7. TÀI KHOẢN ĐĂNG NHẬP MẶC ĐỊNH

| Vai trò (Role) | Tên đăng nhập | Mật khẩu | Số điện thoại | Quyền hạn và phạm vi sử dụng |
| :--- | :--- | :--- | :--- | :--- |
| **Quản trị viên (Admin)** | `admin` | `123456` | `0901234567` | Toàn quyền quản trị hệ thống, sản phẩm & biến thể màu, kho hàng, banner, cấu hình ngân hàng VietQR, duyệt đơn, quản lý người dùng, xuất PDF |
| **Khách hàng mẫu 1** | `giangnt` | `123456` | `0988111222` | Mua sắm, chọn biến thể màu, trả góp 0%, thanh toán VietQR, theo dõi đơn hàng, gửi đánh giá |
| **Khách hàng mẫu 2** | `minhtuan` | `123456` | `0977333444` | Mua sắm, áp dụng mã ưu đãi, yêu cầu xuất hóa đơn GTGT công ty |

---

## 8. DANH SÁCH MÃ GIẢM GIÁ (VOUCHER) KHẢ DỤNG

| Mã Voucher | Loại giảm | Mức giảm | Điều kiện áp dụng |
| :--- | :--- | :--- | :--- |
| `DMX100K` | Tiền mặt | 100.000 VNĐ | Đơn hàng từ 1.000.000 VNĐ |
| `DMX500K` | Tiền mặt | 500.000 VNĐ | Đơn hàng từ 5.000.000 VNĐ |
| `GIAM1TRIEU` | Tiền mặt | 1.000.000 VNĐ | Đơn hàng từ 15.000.000 VNĐ (Tivi, Tủ lạnh cao cấp) |
| `TRIAN5PT` | Phần trăm | 5% (Tối đa 1.500.000 VNĐ) | Đơn hàng từ 3.000.000 VNĐ |
| `DIENMAYXANH` | Tiền mặt | 200.000 VNĐ | Đơn hàng từ 2.000.000 VNĐ |

---

## 9. QUY TẮC PHÁT TRIỂN VÀ TIÊU CHUẨN THIẾT KẾ

1. **Tuân thủ tiêu chuẩn hình ảnh**: Toàn bộ hình ảnh sản phẩm, biến thể màu sắc, banner quảng cáo và biểu tượng ngành hàng sử dụng nguồn trực tiếp từ CDN chính thức của `dienmayxanh.com` (`cdn.tgdd.vn` và `cdnv2.tgdd.vn`), không sử dụng ảnh vẽ minh họa hoặc vector tự sinh.
2. **Nguyên tắc giao diện chuẩn mực**: Toàn bộ hệ thống không sử dụng biểu tượng cảm xúc (zero emojis) nhằm đảm bảo phong cách làm việc chuyên nghiệp của một sàn thương mại điện tử thực tế.
3. **Responsive Design**: Tối ưu hóa hiển thị trên mọi thiết bị: Desktop màn hình rộng, Tablet và điện thoại thông minh (Mobile).

---

*(Bản quyền mã nguồn dự án thuộc về nhóm phát triển hệ thống Website Thương Mại Điện Tử Điện Máy).*
