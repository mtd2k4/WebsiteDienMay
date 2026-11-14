using System.ComponentModel.DataAnnotations;

namespace WebsiteDienMay.Models
{
    public class HomeViewModel
    {
        public List<Category> Categories { get; set; } = new();
        public List<Brand> Brands { get; set; } = new();
        public List<Product> FlashSaleProducts { get; set; } = new();
        public List<Product> FeaturedProducts { get; set; } = new();
        public Dictionary<Category, List<Product>> CategorySections { get; set; } = new();
        public List<Banner> HeroBanners { get; set; } = new();
        public List<Banner> SubBanners { get; set; } = new();
    }

    public class ProductVariantItem
    {
        public string VariantName { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public decimal OriginalPrice { get; set; }
        public string ImageUrl { get; set; } = string.Empty;
        public int DiscountPercent => (OriginalPrice > 0 && Price < OriginalPrice)
            ? (int)Math.Round((OriginalPrice - Price) / OriginalPrice * 100)
            : 0;

        public static List<ProductVariantItem> ParseVariants(string? rawVariants, decimal defaultPrice, decimal defaultOriginalPrice, string defaultImageUrl = "")
        {
            var result = new List<ProductVariantItem>();
            if (string.IsNullOrWhiteSpace(rawVariants)) return result;

            var lines = rawVariants.Split(new[] { '\r', '\n', ';' }, StringSplitOptions.RemoveEmptyEntries);
            foreach (var line in lines)
            {
                var parts = line.Split('|', StringSplitOptions.TrimEntries);
                if (parts.Length == 0 || string.IsNullOrWhiteSpace(parts[0])) continue;

                var vName = parts[0];
                var vPrice = defaultPrice;
                var vOrigPrice = defaultOriginalPrice;
                var vImageUrl = defaultImageUrl;

                if (parts.Length >= 2 && decimal.TryParse(parts[1].Replace(".", "").Replace(",", "").Trim(), out var parsedPrice) && parsedPrice > 0)
                {
                    vPrice = parsedPrice;
                }
                if (parts.Length >= 3 && decimal.TryParse(parts[2].Replace(".", "").Replace(",", "").Trim(), out var parsedOrig) && parsedOrig > 0)
                {
                    vOrigPrice = parsedOrig;
                }
                else if (vOrigPrice < vPrice)
                {
                    vOrigPrice = Math.Round(vPrice * 1.15m / 10000m, 0) * 10000m;
                }
                if (parts.Length >= 4 && !string.IsNullOrWhiteSpace(parts[3]))
                {
                    vImageUrl = parts[3].Trim();
                }

                result.Add(new ProductVariantItem
                {
                    VariantName = vName,
                    Price = vPrice,
                    OriginalPrice = vOrigPrice,
                    ImageUrl = vImageUrl
                });
            }

            return result;
        }
    }

    public class ProductListViewModel
    {
        public List<Product> Products { get; set; } = new();
        public List<Category> Categories { get; set; } = new();
        public List<Brand> Brands { get; set; } = new();

        public int? SelectedCategoryId { get; set; }
        public Category? CurrentCategory { get; set; }
        public int? SelectedBrandId { get; set; }
        public string? SearchKeyword { get; set; }
        public string? PriceRange { get; set; }
        public string? SortOrder { get; set; }

        public int CurrentPage { get; set; } = 1;
        public int TotalPages { get; set; } = 1;
        public int TotalItems { get; set; }
    }

    public class ProductDetailViewModel
    {
        public Product Product { get; set; } = null!;
        public List<Product> RelatedProducts { get; set; } = new();
        public List<ProductReview> Reviews { get; set; } = new();
        public List<KeyValuePair<string, string>> SpecList { get; set; } = new();
        public List<string> Promotions { get; set; } = new();
        public List<string> GalleryImages { get; set; } = new();
        public List<ProductVariantItem> Variants { get; set; } = new();
    }

    public class CartItemViewModel
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public string ProductCode { get; set; } = string.Empty;
        public string? SelectedVariant { get; set; }
        public string ImageUrl { get; set; } = string.Empty;
        public decimal UnitPrice { get; set; }
        public decimal OriginalPrice { get; set; }
        public int Quantity { get; set; }
        public decimal Total => UnitPrice * Quantity;
    }

    public class CartViewModel
    {
        public const decimal VatRate = 0.08m; // Thuế GTGT 8% áp dụng cho thiết bị điện máy

        public List<CartItemViewModel> Items { get; set; } = new();
        public List<Coupon> AvailableCoupons { get; set; } = new();
        public string? CouponCode { get; set; }
        public string? AppliedCouponDescription { get; set; }
        public decimal DiscountAmount { get; set; }
        public decimal SubTotal => Items.Sum(x => x.Total);
        public decimal TaxableAmount => Math.Max(0, SubTotal - DiscountAmount);
        public decimal VatAmount => Math.Round(TaxableAmount * VatRate, 0);
        public decimal FinalTotal => TaxableAmount + VatAmount;
    }

    public class CheckoutViewModel
    {
        [Required(ErrorMessage = "Vui lòng nhập họ và tên người nhận")]
        public string CustomerName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vui lòng nhập số điện thoại liên hệ")]
        [RegularExpression(@"^0[35789][0-9]{8}$", ErrorMessage = "Số điện thoại nhận hàng không hợp lệ (gồm 10 chữ số, bắt đầu bằng 03, 05, 07, 08 hoặc 09)")]
        public string CustomerPhone { get; set; } = string.Empty;

        public string? CustomerEmail { get; set; }

        public int? ProvinceCode { get; set; }

        [Required(ErrorMessage = "Vui lòng chọn Tỉnh / Thành phố")]
        public string ProvinceName { get; set; } = string.Empty;

        public int? WardCode { get; set; }

        [Required(ErrorMessage = "Vui lòng chọn Phường / Xã")]
        public string WardName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vui lòng nhập số nhà, tên đường cụ thể")]
        public string AddressDetail { get; set; } = string.Empty;

        public string ShippingAddress { get; set; } = string.Empty;

        public string? Note { get; set; }

        public string PaymentMethod { get; set; } = "COD";

        // Cấu hình các phương thức trả góp khi chọn PaymentMethod == "Installment"
        public string InstallmentPartnerType { get; set; } = "CreditCard"; // "CreditCard" hoặc "FinanceCompany"
        public string InstallmentBankOrProvider { get; set; } = "Vietcombank";
        public int InstallmentMonths { get; set; } = 6;
        public int InstallmentPrepayPercent { get; set; } = 0;

        // Cấu hình xuất hóa đơn GTGT (VAT) công ty
        public bool RequireVatInvoice { get; set; } = false;
        public string? CompanyName { get; set; }
        public string? CompanyTaxCode { get; set; }
        public string? CompanyAddress { get; set; }
        public string? CompanyEmail { get; set; }

        public string? CouponCode { get; set; }

        public CartViewModel Cart { get; set; } = new();
    }

    public class LoginViewModel
    {
        [Required(ErrorMessage = "Vui lòng nhập tên đăng nhập hoặc số điện thoại")]
        public string Username { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vui lòng nhập mật khẩu")]
        public string Password { get; set; } = string.Empty;

        public string? ReturnUrl { get; set; }
    }

    public class RegisterViewModel
    {
        [Required(ErrorMessage = "Vui lòng nhập họ và tên")]
        public string FullName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vui lòng nhập tên đăng nhập")]
        public string Username { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vui lòng nhập địa chỉ Email")]
        [EmailAddress(ErrorMessage = "Email không hợp lệ")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vui lòng nhập số điện thoại")]
        [RegularExpression(@"^0[35789][0-9]{8}$", ErrorMessage = "Số điện thoại không hợp lệ (gồm đúng 10 chữ số, bắt đầu bằng 03, 05, 07, 08 hoặc 09)")]
        public string Phone { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vui lòng nhập địa chỉ nhận hàng")]
        public string Address { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vui lòng nhập mật khẩu")]
        [MinLength(6, ErrorMessage = "Mật khẩu tối thiểu 6 ký tự")]
        public string Password { get; set; } = string.Empty;

        [Compare("Password", ErrorMessage = "Mật khẩu xác nhận không khớp")]
        public string ConfirmPassword { get; set; } = string.Empty;
    }

    public class AdminDashboardViewModel
    {
        public int TotalProducts { get; set; }
        public int TotalCategories { get; set; }
        public int TotalBrands { get; set; }
        public int TotalOrders { get; set; }
        public int PendingOrders { get; set; }
        public int CompletedOrders { get; set; }
        public int TotalUsers { get; set; }
        public decimal TotalRevenue { get; set; }
        public List<Order> RecentOrders { get; set; } = new();
        public List<Product> TopSellingProducts { get; set; } = new();
        public List<Product> LowStockProducts { get; set; } = new();
    }

    public class WarehouseViewModel
    {
        public List<Product> Products { get; set; } = new();
        public List<Product> AllProductsForSelect { get; set; } = new();
        public List<Category> Categories { get; set; } = new();
        public List<StockImport> RecentImports { get; set; } = new();

        public int? SelectedCategoryId { get; set; }
        public int? PreselectedProductId { get; set; }
        public string? StockFilter { get; set; }
        public string? Keyword { get; set; }

        public int TotalUnitsInStock { get; set; }
        public decimal TotalInventoryValue { get; set; }
        public int LowStockProductCount { get; set; }
        public int OutOfStockProductCount { get; set; }
        public int TotalImportReceipts { get; set; }

        // Thống kê giá trị Nhập kho & Thuế GTGT đầu vào (VAT 8%)
        public decimal TotalImportSubTotal { get; set; }
        public decimal TotalImportVatAmount { get; set; }
        public decimal TotalImportWithVat { get; set; }

        // Thống kê giá trị Xuất hàng & Thuế GTGT đầu ra (VAT 8%)
        public decimal TotalExportSubTotal { get; set; }
        public decimal TotalExportVatAmount { get; set; }
        public decimal TotalExportWithVat { get; set; }
    }

    public class ProductCompareViewModel
    {
        public List<Product> Products { get; set; } = new();
        public List<string> SpecKeys { get; set; } = new();
        public Dictionary<int, Dictionary<string, string>> ProductSpecs { get; set; } = new();
        public List<Product> CandidateProducts { get; set; } = new();
        public Category? Category { get; set; }
    }

    public class LiveSearchResultItem
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Code { get; set; } = string.Empty;
        public string Slug { get; set; } = string.Empty;
        public string ImageUrl { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public string FormattedPrice { get; set; } = string.Empty;
        public decimal OriginalPrice { get; set; }
        public string FormattedOriginalPrice { get; set; } = string.Empty;
        public int DiscountPercent { get; set; }
        public string CategoryName { get; set; } = string.Empty;
        public string BrandName { get; set; } = string.Empty;
        public int StockQuantity { get; set; }
        public bool IsInStock { get; set; }
    }

    public class PrintInvoiceViewModel
    {
        public Order Order { get; set; } = null!;
        public string AmountInWords { get; set; } = string.Empty;
        public string StoreName { get; set; } = "HỆ THỐNG SIÊU THỊ ĐIỆN MÁY XANH";
        public string StoreAddress { get; set; } = "128 Trần Quang Khải, P. Tân Định, Quận 1, TP. Hồ Chí Minh";
        public string StoreHotline { get; set; } = "1800 1061 (Miễn phí từ 7:30 - 22:00)";
        public string StoreWebsite { get; set; } = "https://www.dienmayxanh.com";
        public string StoreTaxCode { get; set; } = "0303217354";
    }

    public class PrintStockImportViewModel
    {
        public StockImport StockImport { get; set; } = null!;
        public decimal SubTotal => StockImport.ImportPrice * StockImport.Quantity;
        public decimal VatAmount => Math.Round(SubTotal * 0.08m, 0);
        public decimal TotalAmount => SubTotal + VatAmount;
        public string AmountInWords { get; set; } = string.Empty;
        public string StoreName { get; set; } = "TRUNG TÂM ĐIỀU VẬN & KHO HÀNG ĐIỆN MÁY XANH";
        public string StoreAddress { get; set; } = "128 Trần Quang Khải, P. Tân Định, Quận 1, TP. Hồ Chí Minh";
        public string StoreHotline { get; set; } = "1800 1061";
        public string StoreTaxCode { get; set; } = "0303217354";
    }

    public class DailyRevenueItem
    {
        public string DateString { get; set; } = string.Empty;
        public string FullDateString { get; set; } = string.Empty;
        public DateTime Date { get; set; }
        public int OrdersCount { get; set; }
        public int ProductsCount { get; set; }
        public decimal GrossRevenue { get; set; }
        public decimal DiscountAmount { get; set; }
        public decimal NetRevenue { get; set; }
        public decimal VatAmount { get; set; }
        public decimal TotalWithVat { get; set; }
        public decimal EstimatedProfit { get; set; }
    }

    public class CategoryRevenueItem
    {
        public int CategoryId { get; set; }
        public string CategoryName { get; set; } = string.Empty;
        public string IconUrl { get; set; } = string.Empty;
        public int TotalQuantity { get; set; }
        public decimal TotalRevenue { get; set; }
        public double Percentage { get; set; }
    }

    public class TopProductRevenueItem
    {
        public int ProductId { get; set; }
        public string ProductCode { get; set; } = string.Empty;
        public string ProductName { get; set; } = string.Empty;
        public string ImageUrl { get; set; } = string.Empty;
        public string CategoryName { get; set; } = string.Empty;
        public decimal UnitPrice { get; set; }
        public int QuantitySold { get; set; }
        public decimal TotalRevenue { get; set; }
        public double RevenuePercentage { get; set; }
    }

    public class RevenueReportViewModel
    {
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
        public string QuickFilter { get; set; } = "30days";
        public int? SelectedCategoryId { get; set; }
        public string SelectedStatus { get; set; } = "valid";

        public decimal GrossRevenue { get; set; }
        public decimal TotalDiscount { get; set; }
        public decimal NetRevenue { get; set; }
        public decimal TotalOutputVat { get; set; }
        public decimal TotalRevenueWithVat { get; set; }
        public decimal TotalStockImportSubTotal { get; set; }
        public decimal TotalInputVat { get; set; }
        public decimal TotalStockImportWithVat { get; set; }
        public int TotalOrders { get; set; }
        public int TotalSoldProducts { get; set; }
        public decimal AverageOrderValue { get; set; }
        public decimal EstimatedCost { get; set; }
        public decimal EstimatedGrossProfit { get; set; }
        public double ProfitMarginPercent { get; set; }

        public List<DailyRevenueItem> DailyRevenues { get; set; } = new();
        public List<CategoryRevenueItem> CategoryRevenues { get; set; } = new();
        public List<TopProductRevenueItem> TopProducts { get; set; } = new();
        public List<Order> Orders { get; set; } = new();
        public List<Category> Categories { get; set; } = new();

        public string ReportCode { get; set; } = string.Empty;
        public DateTime ExportedAt { get; set; } = DateTime.Now;
        public string ExportedBy { get; set; } = "Quản trị viên hệ thống";
    }
}

