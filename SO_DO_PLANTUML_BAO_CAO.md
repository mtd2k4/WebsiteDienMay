# TỔNG HỢP 10 SƠ ĐỒ PLANTUML (TỐI ƯU CHO DRAW.IO / DIAGRAMS.NET)

Toàn bộ 10 sơ đồ dưới đây đã được chuẩn hóa cú pháp tương thích 100% với trình biên dịch PlantUML của **draw.io (diagrams.net)** (loại bỏ `package` lồng nhau và mã màu `#HEX` trên `package` gây cảnh báo, bố trí Actor 2 bên Trái – Phải chống đè chữ).

---

## 1. Hình 2.1 (Mục 2.1.2): Sơ đồ kiến trúc ASP.NET Core 8.0 MVC của hệ thống Website Thương mại điện tử Điện Máy Xanh

```plantuml
@startuml
top to bottom direction
skinparam shadowing false

actor "Khách hàng / Quản trị viên\n(Trình duyệt Web)" as Client

rectangle "Tầng Giao diện (Presentation Layer - Razor Views)" as ViewLayer {
    usecase "Storefront Views\n(Home, Product, Cart, Account, Post, Contact)" as StoreViews
    usecase "Admin CMS Views\n(Dashboard, Products, Inventory, Orders, Banners, BankConfig)" as AdminViews
    usecase "Client Scripts & UI\n(Bootstrap 5.3, Đổi ảnh biến thể, Bảng tính Trả góp, html2pdf.js)" as ClientJS
}

rectangle "Tầng Điều phối & Nghiệp vụ (Controller Layer - ASP.NET Core 8.0)" as ControllerLayer {
    usecase "Middleware Pipeline\n(Routing -> Session -> Cookie Auth -> RBAC Authorization)" as Middleware
    usecase "Storefront Controllers\n(Home, Product, Cart, Account, Post, Contact)" as StoreCtrl
    usecase "AdminController\n([Authorize(Roles = 'Admin')])" as AdminCtrl
    usecase "Dịch vụ Cấu hình & Session\n(HeroBannerStore, BankConfigStore, SessionExtensions)" as Services
}

rectangle "Tầng Dữ liệu (Model & Data Access Layer - EF Core 8.0)" as ModelLayer {
    usecase "Entities & ViewModels\n(Product, Category, Brand, Order, StockImport, Coupon...)" as Models
    usecase "ApplicationDbContext & DbInitializer\n(LINQ Queries, Eager Loading, Auto-Seeding)" as DbCtx
    usecase "Microsoft SQL Server (WebsiteDienMayDb)\n& App_Data JSON (hero-banners.json, bank-config.json)" as SQLDb
}

Client --> Middleware : "1. HTTP/HTTPS Request"
Middleware --> StoreCtrl
Middleware --> AdminCtrl
StoreCtrl --> Services
AdminCtrl --> Services
StoreCtrl --> Models
AdminCtrl --> Models
Models --> DbCtx
DbCtx --> SQLDb : "2. Truy vấn SQL / JSON"
StoreCtrl --> StoreViews : "3. Kết xuất View / JSON"
AdminCtrl --> AdminViews : "3. Kết xuất Admin View"
StoreViews --> ClientJS
AdminViews --> ClientJS
ClientJS --> Client : "4. Hiển thị Giao diện & VietQR"
@enduml
```

---

## 2. Hình 3.1 (Mục 3.2.1): Biểu đồ Use Case tổng quát của hệ thống Website Thương mại điện tử Điện Máy Xanh

### Cách 1: Dàn ngang toàn bộ khung hệ thống (`top to bottom direction` - Khách hàng ở trên, Khung Use Case trải ngang ở giữa, Admin ở dưới)

```plantuml
@startuml
top to bottom direction
skinparam shadowing false

actor "Khách vãng lai (Guest)" as Guest
actor "Khách hàng thành viên (Customer)" as Customer
actor "Quản trị viên (Admin)" as Admin

Customer --|> Guest

rectangle "Hệ thống Website Thương mại điện tử Điện Máy Xanh" {
    usecase "Tìm kiếm, lọc &\nĐổi ảnh theo biến thể" as UC_View
    usecase "So sánh thông số\nsản phẩm (Tối đa 4)" as UC_Compare
    usecase "Hỏi đáp Chatbot\n& Gửi liên hệ" as UC_Chatbot
    usecase "Giỏ hàng, Voucher, VAT 8%,\nTrả góp 0% & VietQR" as UC_Checkout
    usecase "Tra cứu đơn hàng\n(Mã đơn + SĐT)" as UC_Track
    usecase "Tài khoản, Hồ sơ\n& Đánh giá 5 sao" as UC_Account

    usecase "Dashboard doanh thu\n& Cảnh báo tồn kho" as UC_Dashboard
    usecase "Quản lý Sản phẩm\n& Ảnh màu biến thể" as UC_ProdMgr
    usecase "Lập Phiếu nhập kho\n& Xuất Hóa đơn PDF" as UC_StockMgr
    usecase "Quản lý Đơn hàng\n& Hoàn kho tự động" as UC_OrderMgr
    usecase "Quản lý Danh mục,\nVoucher & Người dùng" as UC_CatalogMgr
    usecase "Quản lý Banner &\nNgân hàng VietQR" as UC_ConfigMgr
}

Guest --> UC_View
Guest --> UC_Compare
Guest --> UC_Chatbot
Guest --> UC_Checkout
Guest --> UC_Track
Customer --> UC_Account

UC_Dashboard <-- Admin
UC_ProdMgr <-- Admin
UC_StockMgr <-- Admin
UC_OrderMgr <-- Admin
UC_CatalogMgr <-- Admin
UC_ConfigMgr <-- Admin
@enduml
```

### Cách 2: Chia 2 khung Phân hệ Khách hàng (Trái) và Phân hệ Quản trị (Phải) nằm ngang cạnh nhau

```plantuml
@startuml
top to bottom direction
skinparam shadowing false

actor "Khách vãng lai\n(Guest)" as Guest
actor "Khách hàng thành viên\n(Customer)" as Customer
actor "Quản trị viên\n(Admin)" as Admin

Customer --|> Guest

rectangle "Phân hệ Khách hàng (Storefront UX)" {
    usecase "Tìm kiếm, lọc &\nĐổi ảnh biến thể" as UC_View
    usecase "So sánh thông số\nsản phẩm" as UC_Compare
    usecase "Hỏi đáp Chatbot\n& Liên hệ" as UC_Chatbot
    usecase "Đặt hàng, VAT 8%,\nTrả góp & VietQR" as UC_Checkout
    usecase "Tra cứu\nđơn hàng" as UC_Track
    usecase "Tài khoản &\nĐánh giá 5 sao" as UC_Account
}

rectangle "Phân hệ Quản trị (Admin CMS - /Admin)" {
    usecase "Dashboard doanh thu\n& Cảnh báo tồn kho" as UC_Dashboard
    usecase "Quản lý Sản phẩm\n& Ảnh biến thể" as UC_ProdMgr
    usecase "Nhập kho khép kín\n& Xuất PDF" as UC_StockMgr
    usecase "Quản lý Đơn hàng\n& Hoàn kho" as UC_OrderMgr
    usecase "Danh mục, Voucher\n& Người dùng" as UC_CatalogMgr
    usecase "Quản lý Banner &\nNgân hàng VietQR" as UC_ConfigMgr
}

Guest --> UC_View
Guest --> UC_Compare
Guest --> UC_Chatbot
Guest --> UC_Checkout
Guest --> UC_Track
Customer --> UC_Account

Admin --> UC_Dashboard
Admin --> UC_ProdMgr
Admin --> UC_StockMgr
Admin --> UC_OrderMgr
Admin --> UC_CatalogMgr
Admin --> UC_ConfigMgr
@enduml
```

---

## 3. Hình 3.2 (Mục 3.2.1): Biểu đồ Use Case chi tiết Phân hệ Khách hàng (Storefront UX)

```plantuml
@startuml
left to right direction
skinparam shadowing false

actor "Khách vãng lai (Guest)" as Guest
actor "Khách hàng thành viên (Customer)" as Customer

Customer --|> Guest

rectangle "Phân hệ Khách hàng (Storefront UX - Điện Máy Xanh)" {
    usecase "Duyệt danh mục & Lọc sản phẩm" as UC1
    usecase "Gợi ý tìm kiếm trực tiếp\n(Live Search Suggestions)" as UC1_Sub1
    usecase "Xem chi tiết sản phẩm" as UC2
    usecase "Chọn biến thể (Màu sắc / Dung lượng)\nđổi ảnh Preview & giá bán" as UC2_Sub1
    usecase "So sánh thông số kỹ thuật\n(ProductSpecs 2-4 sản phẩm)" as UC3
    usecase "Tư vấn tự động qua Chatbot\n& Widget Hotline/Zalo" as UC4
    usecase "Quản lý Giỏ hàng & Đặt hàng" as UC5
    usecase "Áp dụng Mã giảm giá (Voucher)\n& Tự động tính thuế VAT 8%" as UC5_Sub1
    usecase "Chọn gói Trả góp 0%\n(Công ty TC / Thẻ tín dụng / Mua trước trả sau)" as UC5_Sub2
    usecase "Thanh toán Chuyển khoản\n(Sinh mã VietQR động)" as UC5_Sub3
    usecase "Tra cứu hành trình đơn hàng 4 bước\n(Mã đơn + SĐT)" as UC6
    usecase "Đăng ký / Đăng nhập\n(Kiểm tra SĐT 10 số & chống trùng lặp)" as UC7
    usecase "Quản lý hồ sơ, Đổi mật khẩu\n& Xem lịch sử đơn hàng" as UC8
    usecase "Gửi đánh giá 5 sao & Bình luận" as UC9

    UC1 ..> UC1_Sub1 : "<<include>>"
    UC2 <.. UC2_Sub1 : "<<extend>>"
    UC5 ..> UC5_Sub1 : "<<include>>"
    UC5 <.. UC5_Sub2 : "<<extend>>"
    UC5 <.. UC5_Sub3 : "<<extend>>"
    UC8 ..> UC7 : "<<include>>"
    UC9 ..> UC7 : "<<include>>"
}

Guest --> UC1
Guest --> UC2
Guest --> UC3
Guest --> UC4
Guest --> UC5
Guest --> UC6
Guest --> UC7

Customer --> UC8
Customer --> UC9
@enduml
```

---

## 4. Hình 3.3 (Mục 3.2.1): Biểu đồ Use Case chi tiết Phân hệ Quản trị (Admin CMS)

```plantuml
@startuml
left to right direction
skinparam shadowing false

actor "Quản trị viên (Admin)" as Admin

rectangle "Phân hệ Quản trị Doanh nghiệp (Admin CMS - /Admin)" {
    usecase "Đăng nhập & Kiểm tra quyền Admin\n([Authorize(Roles='Admin')])" as UC_Auth
    usecase "Xem Dashboard thống kê KPI doanh thu\n& Cảnh báo sản phẩm sắp hết hàng" as UC_Dash
    usecase "Quản lý Sản phẩm điện máy\n(Khóa sửa tay số lượng tồn kho)" as UC_Prod
    usecase "Cấu hình Thông số kỹ thuật\n& Biến thể kèm tải ảnh màu sắc" as UC_Var
    usecase "Quản lý Kho hàng & Lập Phiếu nhập kho\n(Tự động cộng dồn tồn kho)" as UC_Stock
    usecase "Xem chi tiết Phiếu nhập kho\n& Xuất Hóa đơn nhập hàng PDF" as UC_Pdf
    usecase "Quản lý Đơn đặt hàng\n& Cập nhật trạng thái thanh toán" as UC_Order
    usecase "Tự động hoàn trả số lượng tồn kho\nkhi chuyển trạng thái Hủy đơn" as UC_RefundStock
    usecase "Quản lý Danh mục, Thương hiệu,\nTin tức & Hộp thư liên hệ" as UC_Catalog
    usecase "Quản lý Tài khoản người dùng\n(Khóa / Mở khóa / Xóa an toàn)" as UC_Users
    usecase "Quản lý Mã giảm giá (Voucher)" as UC_Coupon
    usecase "Quản lý Banner quảng cáo trang chủ\n& Cấu hình Ngân hàng VietQR động" as UC_Config

    UC_Dash ..> UC_Auth : "<<include>>"
    UC_Prod ..> UC_Auth : "<<include>>"
    UC_Prod ..> UC_Var : "<<include>>"
    UC_Stock ..> UC_Auth : "<<include>>"
    UC_Stock <.. UC_Pdf : "<<extend>>"
    UC_Order ..> UC_Auth : "<<include>>"
    UC_Order <.. UC_RefundStock : "<<extend>>"
    UC_Users ..> UC_Auth : "<<include>>"
    UC_Coupon ..> UC_Auth : "<<include>>"
    UC_Config ..> UC_Auth : "<<include>>"
}

Admin --> UC_Dash
Admin --> UC_Prod
Admin --> UC_Stock
Admin --> UC_Order
Admin --> UC_Catalog
Admin --> UC_Users
Admin --> UC_Coupon
Admin --> UC_Config
@enduml
```

---

## 5. Hình 3.4 (Mục 3.2.3): Biểu đồ hoạt động quy trình Đặt hàng, áp dụng Mã giảm giá (Voucher), tính Trả góp 0% và VietQR động

```plantuml
@startuml
skinparam shadowing false

start
:Khách hàng xem chi tiết sản phẩm & Chọn biến thể\n(Màu sắc / Dung lượng / Công suất);
:Giao diện tự động đổi ảnh Preview (#mainProductImage)\nvà cập nhật giá bán theo biến thể;
:Thêm sản phẩm vào Giỏ hàng & Nhập Mã giảm giá (Voucher);

if (Mã Voucher hợp lệ & đủ điều kiện?) then (Hợp lệ)
    :Tính số tiền giảm giá (DiscountAmount);
else (Không hợp lệ / Không dùng)
    :DiscountAmount = 0;
endif

:Tính tiền hàng sau giảm: Taxable = Max(0, SubTotal - Discount)\nTự động tính thuế VAT (8%): VatAmount = Round(Taxable * 0.08)\nTổng thanh toán: TotalAmount = Taxable + VatAmount + ShippingFee;
:Khách hàng nhập thông tin nhận hàng (Họ tên, SĐT 10 số, Địa chỉ)\nvà chọn Phương thức thanh toán;

if (Phương thức thanh toán?) then (Trả góp 0% - Installment)
    :Chọn hình thức trả góp (Công ty TC / Thẻ tín dụng / Mua trước trả sau),\nchọn Kỳ hạn (3-12 tháng) & % Trả trước (0%-50%);
    :Hệ thống tự động tính Tiền trả trước & Tiền góp/tháng;
elseif (Chuyển khoản - BankTransfer)
    :Đọc cấu hình từ BankConfigStore\n& Hiển thị mã QR VietQR động với số tiền TotalAmount;
else (Tiền mặt COD / Ví MoMo)
    :Xác nhận thanh toán khi nhận hàng;
endif

:Khách hàng bấm nút "Xác nhận đặt hàng";
:CartController kiểm tra tồn kho (Product.Stock >= Quantity);

if (Đủ số lượng tồn kho?) then (Đủ hàng)
    :Tạo bản ghi Order (kèm ghi chú VAT 8% & gói Trả góp)\nvà lưu danh sách OrderDetails (kèm tên biến thể & ảnh màu);
    :Khấu trừ tồn kho (Product.Stock -= Quantity)\nvà tăng lượt bán (SoldCount += Quantity);
    :Xóa Session Giỏ hàng & Chuyển tới trang OrderSuccess\n(Hiển thị mã đơn DMX... và mã VietQR động);
    stop
else (Không đủ tồn kho)
    :Thông báo lỗi sản phẩm không đủ tồn kho\nvà quay lại trang Checkout;
    stop
endif
@enduml
```

---

## 6. Hình 3.5 (Mục 3.2.3): Biểu đồ hoạt động quy trình Quản lý nhập kho khép kín (Khóa sửa tay tồn kho) và Xuất Phiếu nhập kho PDF

```plantuml
@startuml
skinparam shadowing false

start
:Quản trị viên (Admin) truy cập phân hệ Quản trị (/Admin);

if (Thao tác cần thực hiện?) then (Sửa thông tin sản phẩm)
    :Mở màn hình Sửa sản phẩm (/Admin/EditProduct/{id});
    :Hệ thống khóa ô "Số lượng tồn kho" ở chế độ Chỉ đọc (Readonly)\nvà giữ nguyên giá trị Stock hiện có trong CSDL;
    :Lưu thông tin sản phẩm, thông số kỹ thuật\nvà danh sách biến thể kèm ảnh màu sắc;
    stop
else (Nhập thêm hàng vào kho)
    :Mở màn hình Lập phiếu nhập kho (/Admin/CreateStockImport);
    :Nhập tên Nhà cung cấp, SĐT liên hệ, Ghi chú lô hàng\nvà thêm các dòng Sản phẩm (Số lượng nhập, Đơn giá vốn);
    :Bấm nút "Lưu phiếu nhập kho";
    if (Có ít nhất 1 sản phẩm có Số lượng > 0?) then (Hợp lệ)
        :Sinh mã chứng từ lô nhập (BatchCode = PNK + yyyyMMddHHmmss);
        :Lưu các bản ghi vào bảng StockImports\n(Lưu vết Nhà cung cấp, Giá nhập, Người lập, Ngày lập);
        :Tự động cộng dồn số lượng tồn kho:\nProduct.Stock = Product.Stock + Quantity;
        :Admin mở chi tiết chứng từ (/Admin/InventoryDetail/{id})\nvà bấm nút "Xuất hóa đơn PDF";
        :Kết xuất chứng từ chuẩn A4 qua thư viện html2pdf.js\n(Phieu_Nhap_Kho_PNK...pdf) có bảng kê & khung chữ ký;
        stop
    else (Không hợp lệ)
        :Hiển thị thông báo lỗi yêu cầu chọn mặt hàng hợp lệ;
        stop
    endif
endif
@enduml
```

---

## 7. Hình 3.6 (Mục 3.2.4): Biểu đồ tuần tự xử lý đơn hàng, kiểm tra Voucher, tính thuế VAT 8% và đồng bộ tồn kho

```plantuml
@startuml
skinparam shadowing false

actor "Khách hàng" as User
participant "Checkout.cshtml\n(Razor View & JS)" as View
participant "CartController\n(.NET 8.0 MVC)" as Ctrl
participant "BankConfigStore\n(App_Data JSON)" as BankStore
participant "ApplicationDbContext\n(EF Core 8.0)" as DbCtx
database "SQL Server\n(WebsiteDienMayDb)" as DB

User -> View : 1. Truy cập trang Thanh toán (/Cart/Checkout)
View -> Ctrl : 2. GET /Cart/Checkout
Ctrl -> BankStore : 3. GetConfig() đọc cấu hình ngân hàng VietQR
BankStore --> Ctrl : 4. Trả về BankAccountConfig
Ctrl --> View : 5. Hiển thị Giỏ hàng, Thuế VAT 8%,\nBảng tính Trả góp 0% & Mã VietQR động
User -> View : 6. Nhập thông tin nhận hàng, chọn Phương thức\nthanh toán (COD / VietQR / Trả góp) & bấm Đặt hàng
View -> Ctrl : 7. POST /Cart/PlaceOrder(CheckoutViewModel)
activate Ctrl
Ctrl -> DbCtx : 8. Kiểm tra mã Coupon (nếu có)
DbCtx -> DB : 9. SELECT * FROM Coupons WHERE Code = @code
DB --> DbCtx : 10. Trả về thông tin Coupon
Ctrl -> Ctrl : 11. Tính Taxable = SubTotal - Discount\nVatAmount = Round(Taxable * 0.08)\nTotalAmount = Taxable + VatAmount + ShippingFee
loop Duyệt từng mặt hàng trong Giỏ hàng (CartItem)
    Ctrl -> DbCtx : 12. FindAsync(item.ProductId)
    DbCtx -> DB : 13. SELECT * FROM Products WHERE Id = @id
    DB --> DbCtx : 14. Trả về Product (Kiểm tra Stock >= Quantity)
end
alt Đủ tồn kho cho toàn bộ sản phẩm
    Ctrl -> DbCtx : 15. Tạo Order (OrderCode = DMX...) & OrderDetails\n(Lưu kèm Variant & ảnh biến thể màu sắc)
    Ctrl -> DbCtx : 16. Cập nhật Product.Stock -= Quantity,\nProduct.SoldCount += Quantity, Coupon.UsedCount++
    DbCtx -> DB : 17. INSERT Orders, OrderDetails & UPDATE Products
    DB --> DbCtx : 18. Xác nhận lưu giao dịch thành công
    Ctrl -> Ctrl : 19. Xóa CART_SESSION & COUPON_SESSION
    Ctrl --> View : 20. RedirectToAction("OrderSuccess", { orderCode })
    View --> User : 21. Hiển thị trang Đặt hàng thành công\nkèm Mã đơn hàng & Mã QR VietQR động
else Không đủ tồn kho
    Ctrl --> View : 22. Báo lỗi sản phẩm không đủ tồn kho & Quay lại Checkout
end
deactivate Ctrl
@enduml
```

---

## 8. Hình 3.7 (Mục 3.2.5): Biểu đồ lớp (Class Diagram) của hệ thống Website Thương mại điện tử Điện Máy Xanh

```plantuml
@startuml
skinparam shadowing false
skinparam classAttributeIconSize 0

class Category {
    +Id : int
    +Name : string
    +Slug : string
    +Icon : string
    +DisplayOrder : int
    +IsActive : bool
    +ParentId : int?
}

class Brand {
    +Id : int
    +Name : string
    +Slug : string
    +LogoUrl : string
    +IsActive : bool
}

class Product {
    +Id : int
    +Name : string
    +Slug : string
    +SKU : string
    +Price : decimal
    +OldPrice : decimal?
    +DiscountPercent : int
    +Stock : int
    +SoldCount : int
    +ImageUrl : string
    +WarrantyInfo : string
    +VariantsJson : string
    +IsFlashSale : bool
    +IsInstallment : bool
    +CategoryId : int
    +BrandId : int
}

class ProductVariantItem {
    +Name : string
    +Price : decimal
    +OldPrice : decimal?
    +ImageUrl : string
}

class ProductSpec {
    +Id : int
    +ProductId : int
    +SpecName : string
    +SpecValue : string
    +DisplayOrder : int
}

class ProductImage {
    +Id : int
    +ProductId : int
    +ImageUrl : string
    +DisplayOrder : int
}

class StockImport {
    +Id : int
    +BatchCode : string
    +ProductId : int
    +Quantity : int
    +ImportPrice : decimal
    +Supplier : string
    +SupplierPhone : string
    +CreatedBy : string
    +CreatedAt : DateTime
}

class User {
    +Id : int
    +FullName : string
    +Email : string
    +Phone : string
    +PasswordHash : string
    +Role : string
    +IsActive : bool
}

class Order {
    +Id : int
    +OrderCode : string
    +UserId : int?
    +CustomerName : string
    +Phone : string
    +ShippingAddress : string
    +Note : string
    +SubTotal : decimal
    +DiscountAmount : decimal
    +TotalAmount : decimal
    +PaymentMethod : string
    +PaymentStatus : string
    +OrderStatus : string
    +CreatedAt : DateTime
}

class OrderDetail {
    +Id : int
    +OrderId : int
    +ProductId : int
    +ProductName : string
    +ProductImage : string
    +Variant : string
    +UnitPrice : decimal
    +Quantity : int
    +TotalPrice : decimal
}

class Coupon {
    +Id : int
    +Code : string
    +DiscountPercent : int
    +MaxDiscountAmount : decimal
    +MinOrderAmount : decimal
    +UsageLimit : int
    +UsedCount : int
    +ExpiryDate : DateTime
}

class Review {
    +Id : int
    +ProductId : int
    +UserId : int?
    +ReviewerName : string
    +Rating : int
    +Comment : string
}

class BankAccountConfig {
    +BankCode : string
    +BankName : string
    +AccountNumber : string
    +AccountName : string
    +Branch : string
    +Template : string
    +TransferPrefix : string
}

Category "1" --> "0..*" Product : "phân loại"
Brand "1" --> "0..*" Product : "sản xuất"
Product "1" *-- "0..*" ProductVariantItem : "VariantsJson"
Product "1" *-- "0..*" ProductSpec : "thông số"
Product "1" *-- "0..*" ProductImage : "thư viện ảnh"
Product "1" --> "0..*" StockImport : "nhập kho"
Product "1" --> "0..*" Review : "đánh giá"
User "1" --> "0..*" Order : "đặt mua"
User "1" --> "0..*" Review : "viết"
Order "1" *-- "1..*" OrderDetail : "bao gồm"
Product "1" --> "0..*" OrderDetail : "tham chiếu"
Order ..> Coupon : "áp dụng mã"
Order ..> BankAccountConfig : "sinh mã VietQR"
@enduml
```

---

## 9. Hình 3.8 (Mục 3.3.1): Sơ đồ thực thể liên kết (ERD) cơ sở dữ liệu `WebsiteDienMayDb`

```plantuml
@startuml
hide circle
skinparam linetype ortho
skinparam shadowing false

class "Categories" as Categories {
    +Id : int <<PK>>
    --
    Name : nvarchar(100)
    Slug : nvarchar(100) <<UQ>>
    Icon : nvarchar(100)
    DisplayOrder : int
    IsActive : bit
    ParentId : int <<FK>>
}

class "Brands" as Brands {
    +Id : int <<PK>>
    --
    Name : nvarchar(100)
    Slug : nvarchar(100) <<UQ>>
    LogoUrl : nvarchar(255)
    IsActive : bit
}

class "Products" as Products {
    +Id : int <<PK>>
    --
    Name : nvarchar(250)
    Slug : nvarchar(250) <<UQ>>
    SKU : nvarchar(50)
    Price : decimal(18,2)
    OldPrice : decimal(18,2)
    DiscountPercent : int
    Stock : int
    SoldCount : int
    ImageUrl : nvarchar(255)
    WarrantyInfo : nvarchar(200)
    VariantsJson : nvarchar(MAX)
    IsFeatured : bit
    IsFlashSale : bit
    IsInstallment : bit
    IsActive : bit
    CategoryId : int <<FK>>
    BrandId : int <<FK>>
}

class "ProductImages" as ProductImages {
    +Id : int <<PK>>
    --
    ProductId : int <<FK>>
    ImageUrl : nvarchar(255)
    DisplayOrder : int
}

class "ProductSpecs" as ProductSpecs {
    +Id : int <<PK>>
    --
    ProductId : int <<FK>>
    SpecName : nvarchar(100)
    SpecValue : nvarchar(300)
    DisplayOrder : int
}

class "StockImports" as StockImports {
    +Id : int <<PK>>
    --
    BatchCode : nvarchar(30)
    ProductId : int <<FK>>
    Quantity : int
    ImportPrice : decimal(18,2)
    Supplier : nvarchar(200)
    SupplierPhone : nvarchar(30)
    Note : nvarchar(500)
    CreatedBy : nvarchar(100)
    CreatedAt : datetime2
}

class "Users" as Users {
    +Id : int <<PK>>
    --
    FullName : nvarchar(100)
    Email : nvarchar(150) <<UQ>>
    Phone : nvarchar(20) <<UQ>>
    PasswordHash : nvarchar(255)
    Address : nvarchar(300)
    Role : nvarchar(20)
    IsActive : bit
    CreatedAt : datetime2
}

class "Orders" as Orders {
    +Id : int <<PK>>
    --
    OrderCode : nvarchar(30) <<UQ>>
    UserId : int <<FK>>
    CustomerName : nvarchar(100)
    Phone : nvarchar(20)
    Email : nvarchar(150)
    ShippingAddress : nvarchar(300)
    Note : nvarchar(500)
    SubTotal : decimal(18,2)
    ShippingFee : decimal(18,2)
    DiscountAmount : decimal(18,2)
    TotalAmount : decimal(18,2)
    CouponCode : nvarchar(50)
    PaymentMethod : nvarchar(50)
    PaymentStatus : nvarchar(30)
    OrderStatus : nvarchar(30)
    CreatedAt : datetime2
}

class "OrderDetails" as OrderDetails {
    +Id : int <<PK>>
    --
    OrderId : int <<FK>>
    ProductId : int <<FK>>
    ProductName : nvarchar(250)
    ProductImage : nvarchar(255)
    Variant : nvarchar(100)
    UnitPrice : decimal(18,2)
    Quantity : int
    TotalPrice : decimal(18,2)
}

class "Reviews" as Reviews {
    +Id : int <<PK>>
    --
    ProductId : int <<FK>>
    UserId : int <<FK>>
    ReviewerName : nvarchar(100)
    Rating : int
    Comment : nvarchar(1000)
    IsApproved : bit
    CreatedAt : datetime2
}

class "Coupons" as Coupons {
    +Id : int <<PK>>
    --
    Code : nvarchar(50) <<UQ>>
    Description : nvarchar(250)
    DiscountPercent : int
    MaxDiscountAmount : decimal(18,2)
    MinOrderAmount : decimal(18,2)
    UsageLimit : int
    UsedCount : int
    ExpiryDate : datetime2
    IsActive : bit
}

class "Posts" as Posts {
    +Id : int <<PK>>
    --
    Title : nvarchar(250)
    Slug : nvarchar(250)
    Category : nvarchar(100)
    IsPublished : bit
    CreatedAt : datetime2
}

class "ContactMessages" as ContactMessages {
    +Id : int <<PK>>
    --
    FullName : nvarchar(100)
    Email : nvarchar(150)
    Phone : nvarchar(20)
    Subject : nvarchar(200)
    Message : nvarchar(2000)
    IsRead : bit
}

Categories "1" --> "0..*" Categories : "ParentId"
Categories "1" --> "0..*" Products : "CategoryId"
Brands "1" --> "0..*" Products : "BrandId"
Products "1" --> "0..*" ProductImages : "ProductId"
Products "1" --> "0..*" ProductSpecs : "ProductId"
Products "1" --> "0..*" StockImports : "ProductId"
Products "1" --> "0..*" Reviews : "ProductId"
Products "1" --> "0..*" OrderDetails : "ProductId"
Users "1" --> "0..*" Orders : "UserId"
Users "1" --> "0..*" Reviews : "UserId"
Orders "1" --> "1..*" OrderDetails : "OrderId"
Coupons "0..1" ..> "0..*" Orders : "CouponCode"
@enduml
```

---

## 10. Hình 3.9 (Mục 3.5.1): Sơ đồ điều hướng tổng thể (Sitemap) của Storefront và Admin CMS

```plantuml
@startuml
top to bottom direction
skinparam shadowing false

rectangle "Hệ thống Website Thương mại điện tử Điện Máy Xanh" as Root

rectangle "1. Phân hệ Khách hàng (Storefront UX)" as Store {
    usecase "Trang chủ (/)\nBanner động, Flash Sale, 10 Ngành hàng" as S1
    usecase "Danh mục & Tìm kiếm (/danh-muc/{slug})\nLive Search, Lọc giá, Thương hiệu, Trả góp 0%" as S2
    usecase "Chi tiết sản phẩm (/san-pham/{slug})\nChọn biến thể đổi ảnh màu sắc, Thông số, Đánh giá" as S3
    usecase "So sánh sản phẩm (/so-sanh-san-pham)\nĐối chiếu cấu hình 2-4 sản phẩm" as S4
    usecase "Giỏ hàng & Thanh toán (/Cart/Checkout)\nVoucher, Thuế VAT 8%, Trả góp 0%, VietQR động" as S5
    usecase "Tra cứu đơn hàng (/Cart/TrackOrder)\nHành trình 4 bước bằng Mã đơn + SĐT" as S6
    usecase "Tài khoản & Tư vấn (/Account, Chatbot)\nĐăng ký SĐT 10 số, Lịch sử đơn, Chatbot 24/7" as S7
}

rectangle "2. Phân hệ Quản trị (Admin CMS - /Admin)" as Admin {
    usecase "Dashboard thống kê (/Admin)\nDoanh thu, Đơn chờ xử lý, Cảnh báo tồn kho" as A1
    usecase "Quản lý Sản phẩm (/Admin/Products)\nKhóa sửa tay tồn kho, Biến thể & Ảnh màu sắc" as A2
    usecase "Quản lý Kho hàng (/Admin/Inventory)\nLập Phiếu nhập kho & Xuất Hóa đơn PDF" as A3
    usecase "Quản lý Đơn hàng (/Admin/Orders)\nDuyệt đơn, Thanh toán & Hoàn kho tự động" as A4
    usecase "Quản lý Danh mục, Thương hiệu, Voucher\n(/Admin/Categories, Brands, Coupons)" as A5
    usecase "Quản lý Banner & Cấu hình Ngân hàng VietQR\n(/Admin/Banners, /Admin/BankConfig)" as A6
    usecase "Quản lý Người dùng, Tin tức & Liên hệ\n(/Admin/Users, Posts, Contacts)" as A7
}

Root --> Store
Root --> Admin
@enduml
```
