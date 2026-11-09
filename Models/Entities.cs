using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebsiteDienMay.Models
{
    public class Category
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập tên danh mục")]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;

        [StringLength(120)]
        public string Slug { get; set; } = string.Empty;

        [StringLength(500)]
        public string? Description { get; set; }

        [StringLength(500)]
        public string IconUrl { get; set; } = string.Empty;

        [StringLength(500)]
        public string? BannerUrl { get; set; }

        public int DisplayOrder { get; set; } = 1;

        public bool IsActive { get; set; } = true;

        public ICollection<Product> Products { get; set; } = new List<Product>();
    }

    public class Brand
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập tên thương hiệu")]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;

        [StringLength(120)]
        public string Slug { get; set; } = string.Empty;

        [StringLength(500)]
        public string LogoUrl { get; set; } = string.Empty;

        [StringLength(100)]
        public string Origin { get; set; } = "Chính hãng";

        public ICollection<Product> Products { get; set; } = new List<Product>();
    }

    public class Product
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập tên sản phẩm")]
        [StringLength(250)]
        public string Name { get; set; } = string.Empty;

        [StringLength(50)]
        public string Code { get; set; } = string.Empty;

        [StringLength(260)]
        public string Slug { get; set; } = string.Empty;

        public int CategoryId { get; set; }
        public Category? Category { get; set; }

        public int BrandId { get; set; }
        public Brand? Brand { get; set; }

        [Column(TypeName = "decimal(18,0)")]
        public decimal OriginalPrice { get; set; }

        [Column(TypeName = "decimal(18,0)")]
        public decimal Price { get; set; }

        public int DiscountPercent
        {
            get
            {
                if (OriginalPrice <= 0 || Price >= OriginalPrice) return 0;
                return (int)Math.Round((OriginalPrice - Price) / OriginalPrice * 100);
            }
        }

        public int StockQuantity { get; set; } = 0;
        public int SoldCount { get; set; } = 0;

        [Required(ErrorMessage = "Vui lòng nhập đường dẫn hình ảnh")]
        [StringLength(600)]
        public string ImageUrl { get; set; } = string.Empty;

        [StringLength(2000)]
        public string? ExtraImages { get; set; }

        [StringLength(500)]
        public string ShortDescription { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;

        public string Specifications { get; set; } = string.Empty;

        // Danh sách biến thể sản phẩm (Mỗi dòng: Tên biến thể | Giá bán | Giá gốc)
        [StringLength(2000)]
        public string? ProductVariants { get; set; }

        [StringLength(1000)]
        public string PromotionText { get; set; } = "Miễn phí công lắp đặt; Bảo hành chính hãng tận nhà; Đổi trả trong 30 ngày nếu lỗi kỹ thuật";

        public int WarrantyMonths { get; set; } = 24;

        public bool IsFlashSale { get; set; } = false;
        public bool IsFeatured { get; set; } = true;
        public bool IsInstallment0Percent { get; set; } = true;

        public double RatingAverage { get; set; } = 4.9;
        public int RatingCount { get; set; } = 42;

        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public bool IsActive { get; set; } = true;

        public ICollection<ProductReview> Reviews { get; set; } = new List<ProductReview>();
        public ICollection<StockImport> StockImports { get; set; } = new List<StockImport>();
    }

    public class User
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập họ tên")]
        [StringLength(120)]
        public string FullName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vui lòng nhập tên đăng nhập")]
        [StringLength(80)]
        public string Username { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vui lòng nhập Email")]
        [StringLength(150)]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vui lòng nhập số điện thoại")]
        [StringLength(20)]
        public string Phone { get; set; } = string.Empty;

        [Required]
        [StringLength(250)]
        public string PasswordHash { get; set; } = string.Empty;

        [StringLength(300)]
        public string Address { get; set; } = string.Empty;

        [StringLength(30)]
        public string Role { get; set; } = "Customer"; // "Admin" hoặc "Customer"

        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public bool IsActive { get; set; } = true;

        public ICollection<Order> Orders { get; set; } = new List<Order>();
    }

    public class Order
    {
        public int Id { get; set; }

        [StringLength(40)]
        public string OrderCode { get; set; } = string.Empty;

        public int? UserId { get; set; }
        public User? User { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập họ tên người nhận")]
        [StringLength(120)]
        public string CustomerName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vui lòng nhập số điện thoại")]
        [StringLength(20)]
        public string CustomerPhone { get; set; } = string.Empty;

        [StringLength(150)]
        public string? CustomerEmail { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập địa chỉ nhận hàng")]
        [StringLength(350)]
        public string ShippingAddress { get; set; } = string.Empty;

        [StringLength(500)]
        public string? Note { get; set; }

        [Column(TypeName = "decimal(18,0)")]
        public decimal SubTotal { get; set; }

        [Column(TypeName = "decimal(18,0)")]
        public decimal DiscountAmount { get; set; }

        [StringLength(50)]
        public string? CouponCode { get; set; }

        [Column(TypeName = "decimal(18,0)")]
        public decimal VatAmount { get; set; }

        [Column(TypeName = "decimal(18,0)")]
        public decimal TotalAmount { get; set; }

        [StringLength(50)]
        public string PaymentMethod { get; set; } = "COD";

        // Thông tin gói trả góp đã chọn (Hình thức, Ngân hàng/Công ty tài chính, Kỳ hạn, Trả trước)
        [StringLength(500)]
        public string? InstallmentInfo { get; set; }

        // Thông tin xuất hóa đơn GTGT (VAT) công ty
        public bool RequireVatInvoice { get; set; } = false;

        [StringLength(200)]
        public string? CompanyName { get; set; }

        [StringLength(50)]
        public string? CompanyTaxCode { get; set; }

        [StringLength(350)]
        public string? CompanyAddress { get; set; }

        [StringLength(150)]
        public string? CompanyEmail { get; set; }

        [StringLength(60)]
        public string PaymentStatus { get; set; } = "Chưa thanh toán";

        [StringLength(60)]
        public string OrderStatus { get; set; } = "Chờ xác nhận";

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public ICollection<OrderDetail> OrderDetails { get; set; } = new List<OrderDetail>();
    }

    public class OrderDetail
    {
        public int Id { get; set; }

        public int OrderId { get; set; }
        public Order? Order { get; set; }

        public int ProductId { get; set; }
        public Product? Product { get; set; }

        [StringLength(250)]
        public string ProductName { get; set; } = string.Empty;

        [StringLength(150)]
        public string? SelectedVariant { get; set; }

        [StringLength(600)]
        public string ProductImage { get; set; } = string.Empty;

        [Column(TypeName = "decimal(18,0)")]
        public decimal UnitPrice { get; set; }

        public int Quantity { get; set; }
    }

    public class ProductReview
    {
        public int Id { get; set; }

        public int ProductId { get; set; }
        public Product? Product { get; set; }

        public int? UserId { get; set; }
        public User? User { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập họ tên")]
        [StringLength(120)]
        public string CustomerName { get; set; } = string.Empty;

        [StringLength(20)]
        public string? CustomerPhone { get; set; }

        [Range(1, 5)]
        public int Rating { get; set; } = 5;

        [Required(ErrorMessage = "Vui lòng nhập nội dung đánh giá")]
        [StringLength(1000)]
        public string Comment { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public bool IsApproved { get; set; } = true;
    }

    public class StockImport
    {
        public int Id { get; set; }

        [StringLength(40)]
        public string ReceiptCode { get; set; } = string.Empty;

        public int ProductId { get; set; }
        public Product? Product { get; set; }

        [Range(1, 100000, ErrorMessage = "Số lượng nhập kho phải lớn hơn 0")]
        public int Quantity { get; set; }

        [Column(TypeName = "decimal(18,0)")]
        public decimal ImportPrice { get; set; }

        public int StockBefore { get; set; }
        public int StockAfter { get; set; }

        [StringLength(150)]
        public string SupplierName { get; set; } = "Nhà phân phối chính hãng";

        [StringLength(500)]
        public string? Note { get; set; }

        [StringLength(120)]
        public string CreatedBy { get; set; } = "Quản trị viên";

        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }

    public class Coupon
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập mã ưu đãi")]
        [StringLength(40)]
        public string Code { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vui lòng nhập mô tả mã giảm giá")]
        [StringLength(200)]
        public string Description { get; set; } = string.Empty;

        // "FixedAmount" (giảm số tiền cố định) hoặc "Percentage" (giảm theo %)
        [StringLength(20)]
        public string DiscountType { get; set; } = "FixedAmount";

        [Column(TypeName = "decimal(18,0)")]
        [Range(1, 100000000, ErrorMessage = "Giá trị giảm phải lớn hơn 0")]
        public decimal DiscountValue { get; set; }

        [Column(TypeName = "decimal(18,0)")]
        public decimal MinOrderAmount { get; set; } = 0;

        [Column(TypeName = "decimal(18,0)")]
        public decimal? MaxDiscountAmount { get; set; }

        public int UsageLimit { get; set; } = 500;

        public int UsedCount { get; set; } = 0;

        public DateTime StartDate { get; set; } = DateTime.Now;

        public DateTime EndDate { get; set; } = DateTime.Now.AddMonths(3);

        public bool IsActive { get; set; } = true;
    }

    public class Banner
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập tiêu đề banner quảng cáo")]
        [StringLength(200)]
        public string Title { get; set; } = string.Empty;

        [StringLength(300)]
        public string? Subtitle { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập đường dẫn hình ảnh banner")]
        [StringLength(600)]
        public string ImageUrl { get; set; } = string.Empty;

        [StringLength(400)]
        public string LinkUrl { get; set; } = "/Product";

        // "Hero" (Banner lớn trượt trên cùng) hoặc "SubBanner" (Banner đôi khuyến mãi phía dưới)
        [StringLength(40)]
        public string Position { get; set; } = "Hero";

        public int DisplayOrder { get; set; } = 1;

        public bool IsActive { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }

    public class BankAccountConfig
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Vui lòng chọn mã ngân hàng VietQR")]
        [StringLength(30)]
        public string BankCode { get; set; } = "MB";

        [Required(ErrorMessage = "Vui lòng nhập tên ngân hàng")]
        [StringLength(150)]
        public string BankName { get; set; } = "MBBank - Ngân hàng TMCP Quân Đội";

        [Required(ErrorMessage = "Vui lòng nhập số tài khoản thụ hưởng")]
        [StringLength(50)]
        public string AccountNumber { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vui lòng nhập tên chủ tài khoản")]
        [StringLength(150)]
        public string AccountHolder { get; set; } = "SIEU THI DIEN MAY XANH";

        [StringLength(150)]
        public string Branch { get; set; } = "Chi nhánh Sở Giao Dịch TP. Hồ Chí Minh";

        // Mẫu VietQR: compact2, compact, print, qr_only
        [StringLength(30)]
        public string QrTemplate { get; set; } = "compact2";

        public bool IsDefault { get; set; } = false;

        public bool IsActive { get; set; } = true;

        public DateTime UpdatedAt { get; set; } = DateTime.Now;
    }
}

