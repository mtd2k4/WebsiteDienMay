using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.EntityFrameworkCore;
using WebsiteDienMay.Data;
using WebsiteDienMay.Models;

namespace WebsiteDienMay.Controllers
{
    public class AdminController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AdminController(ApplicationDbContext context)
        {
            _context = context;
        }

        public override void OnActionExecuting(ActionExecutingContext context)
        {
            var userId = HttpContext.Session.GetInt32("USER_ID");
            var role = HttpContext.Session.GetString("USER_ROLE");

            // Trường hợp 1: Chưa đăng nhập -> Yêu cầu đăng nhập bằng tài khoản Admin
            if (!userId.HasValue)
            {
                TempData["ErrorMessage"] = "Vui lòng đăng nhập bằng tài khoản Quản trị viên (Admin) để truy cập khu vực này.";
                context.Result = new RedirectToActionResult("Login", "Account", new { returnUrl = HttpContext.Request.Path });
                return;
            }

            // Trường hợp 2: Đã đăng nhập nhưng là Khách hàng (Customer) -> Cấm truy cập tuyệt đối
            if (!string.Equals(role, "Admin", StringComparison.OrdinalIgnoreCase))
            {
                TempData["ErrorMessage"] = "Truy cập bị từ chối! Khu vực Quản trị hệ thống chỉ dành riêng cho Quản trị viên (Admin). Khách hàng không có quyền truy cập.";
                context.Result = new RedirectToActionResult("AccessDenied", "Account", null);
                return;
            }

            base.OnActionExecuting(context);
        }

        public async Task<IActionResult> Index()
        {
            var orders = await _context.Orders.OrderByDescending(o => o.CreatedAt).ToListAsync();
            var products = await _context.Products.Include(p => p.Category).Include(p => p.Brand).ToListAsync();

            var vm = new AdminDashboardViewModel
            {
                TotalProducts = products.Count,
                TotalCategories = await _context.Categories.CountAsync(),
                TotalBrands = await _context.Brands.CountAsync(),
                TotalOrders = orders.Count,
                PendingOrders = orders.Count(o => o.OrderStatus == "Chờ xác nhận"),
                CompletedOrders = orders.Count(o => o.OrderStatus == "Hoàn thành"),
                TotalUsers = await _context.Users.CountAsync(),
                TotalRevenue = orders.Where(o => o.OrderStatus != "Đã hủy").Sum(o => o.TotalAmount),
                RecentOrders = orders.Take(8).ToList(),
                TopSellingProducts = products.OrderByDescending(p => p.SoldCount).Take(6).ToList(),
                LowStockProducts = products.OrderBy(p => p.StockQuantity).Take(5).ToList()
            };

            return View(vm);
        }

        // ================= QUẢN LÝ SẢN PHẨM =================
        public async Task<IActionResult> Products(int? categoryId, string? keyword)
        {
            var query = _context.Products
                .Include(p => p.Category)
                .Include(p => p.Brand)
                .AsQueryable();

            if (categoryId.HasValue && categoryId.Value > 0)
            {
                query = query.Where(p => p.CategoryId == categoryId.Value);
            }

            if (!string.IsNullOrWhiteSpace(keyword))
            {
                var kw = keyword.Trim().ToLower();
                query = query.Where(p => p.Name.ToLower().Contains(kw) || p.Code.ToLower().Contains(kw));
            }

            ViewBag.Categories = await _context.Categories.OrderBy(c => c.DisplayOrder).ToListAsync();
            ViewBag.SelectedCategoryId = categoryId;
            ViewBag.Keyword = keyword;

            var list = await query.OrderByDescending(p => p.Id).ToListAsync();
            return View(list);
        }

        [HttpGet]
        public async Task<IActionResult> CreateProduct()
        {
            ViewBag.Categories = await _context.Categories.OrderBy(c => c.DisplayOrder).ToListAsync();
            ViewBag.Brands = await _context.Brands.OrderBy(b => b.Name).ToListAsync();

            var model = new Product
            {
                Code = "DMX-SP" + DateTime.Now.ToString("HHmm"),
                ImageUrl = "https://cdn.tgdd.vn/2026/11/timerseo/365275.jpg",
                StockQuantity = 0,
                SoldCount = 0,
                WarrantyMonths = 24,
                IsActive = true,
                IsFeatured = true,
                IsInstallment0Percent = true
            };
            return View("ProductForm", model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateProduct(Product model)
        {
            ModelState.Remove(nameof(Product.StockQuantity));
            if (!ModelState.IsValid)
            {
                model.StockQuantity = 0;
                ViewBag.Categories = await _context.Categories.OrderBy(c => c.DisplayOrder).ToListAsync();
                ViewBag.Brands = await _context.Brands.OrderBy(b => b.Name).ToListAsync();
                return View("ProductForm", model);
            }

            if (string.IsNullOrWhiteSpace(model.Slug))
            {
                model.Slug = model.Code.ToLower();
            }

            // Không cho phép nhập số lượng tồn kho thủ công khi tạo sản phẩm; chỉ tăng khi nhập kho
            model.StockQuantity = 0;
            model.SoldCount = 0;
            model.CreatedAt = DateTime.Now;
            _context.Products.Add(model);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = $"Đã tạo sản phẩm \"{model.Name}\" (Tồn kho ban đầu: 0). Vui lòng lập phiếu nhập kho để bổ sung số lượng.";
            return RedirectToAction(nameof(Warehouse), new { productId = model.Id });
        }

        [HttpGet]
        public async Task<IActionResult> EditProduct(int id)
        {
            var product = await _context.Products.FindAsync(id);
            if (product == null)
            {
                return NotFound();
            }

            ViewBag.Categories = await _context.Categories.OrderBy(c => c.DisplayOrder).ToListAsync();
            ViewBag.Brands = await _context.Brands.OrderBy(b => b.Name).ToListAsync();
            return View("ProductForm", product);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditProduct(Product model)
        {
            var existing = await _context.Products.FindAsync(model.Id);
            if (existing == null)
            {
                return NotFound();
            }

            ModelState.Remove(nameof(Product.StockQuantity));
            if (!ModelState.IsValid)
            {
                model.StockQuantity = existing.StockQuantity;
                ViewBag.Categories = await _context.Categories.OrderBy(c => c.DisplayOrder).ToListAsync();
                ViewBag.Brands = await _context.Brands.OrderBy(b => b.Name).ToListAsync();
                return View("ProductForm", model);
            }

            existing.Name = model.Name;
            existing.Code = model.Code;
            existing.CategoryId = model.CategoryId;
            existing.BrandId = model.BrandId;
            existing.OriginalPrice = model.OriginalPrice;
            existing.Price = model.Price;
            // Tuyệt đối không chỉnh sửa existing.StockQuantity bằng tay tại đây
            existing.ImageUrl = model.ImageUrl;
            existing.ExtraImages = model.ExtraImages;
            existing.ShortDescription = model.ShortDescription;
            existing.Description = model.Description;
            existing.Specifications = model.Specifications;
            existing.ProductVariants = model.ProductVariants;
            existing.PromotionText = model.PromotionText;
            existing.WarrantyMonths = model.WarrantyMonths;
            existing.IsFlashSale = model.IsFlashSale;
            existing.IsFeatured = model.IsFeatured;
            existing.IsInstallment0Percent = model.IsInstallment0Percent;
            existing.IsActive = model.IsActive;

            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = $"Đã cập nhật thông tin sản phẩm \"{existing.Name}\" thành công.";
            return RedirectToAction(nameof(Products));
        }

        // ================= QUẢN LÝ KHO HÀNG & NHẬP KHO =================
        [HttpGet]
        public async Task<IActionResult> Warehouse(int? categoryId, string? stockFilter, string? keyword, int? productId)
        {
            var allProducts = await _context.Products
                .Include(p => p.Category)
                .Include(p => p.Brand)
                .OrderBy(p => p.StockQuantity)
                .ThenByDescending(p => p.Id)
                .ToListAsync();

            var filtered = allProducts.AsEnumerable();

            if (categoryId.HasValue && categoryId.Value > 0)
            {
                filtered = filtered.Where(p => p.CategoryId == categoryId.Value);
            }

            if (!string.IsNullOrWhiteSpace(stockFilter))
            {
                filtered = stockFilter switch
                {
                    "out" => filtered.Where(p => p.StockQuantity <= 0),
                    "low" => filtered.Where(p => p.StockQuantity > 0 && p.StockQuantity <= 10),
                    "in" => filtered.Where(p => p.StockQuantity > 10),
                    _ => filtered
                };
            }

            if (!string.IsNullOrWhiteSpace(keyword))
            {
                var kw = keyword.Trim().ToLower();
                filtered = filtered.Where(p => p.Name.ToLower().Contains(kw) || p.Code.ToLower().Contains(kw));
            }

            var recentImports = await _context.StockImports
                .Include(si => si.Product)
                .OrderByDescending(si => si.CreatedAt)
                .Take(30)
                .ToListAsync();

            var allStockImports = await _context.StockImports.AsNoTracking().ToListAsync();
            var totalImpSub = allStockImports.Sum(si => si.ImportPrice * si.Quantity);
            var totalImpVat = allStockImports.Sum(si => Math.Round(si.ImportPrice * si.Quantity * 0.08m, 0));

            var validExportOrders = await _context.Orders
                .AsNoTracking()
                .Where(o => o.OrderStatus != "Đã hủy")
                .ToListAsync();
            var totalExpSub = validExportOrders.Sum(o => Math.Max(0, o.SubTotal - o.DiscountAmount));
            var totalExpVat = validExportOrders.Sum(o => o.VatAmount > 0 ? o.VatAmount : Math.Round(Math.Max(0, o.SubTotal - o.DiscountAmount) * 0.08m, 0));

            var vm = new WarehouseViewModel
            {
                Products = filtered.ToList(),
                AllProductsForSelect = allProducts.OrderBy(p => p.Name).ToList(),
                Categories = await _context.Categories.OrderBy(c => c.DisplayOrder).ToListAsync(),
                RecentImports = recentImports,
                SelectedCategoryId = categoryId,
                PreselectedProductId = productId,
                StockFilter = stockFilter ?? "",
                Keyword = keyword,
                TotalUnitsInStock = allProducts.Sum(p => p.StockQuantity),
                TotalInventoryValue = allProducts.Sum(p => p.Price * p.StockQuantity),
                LowStockProductCount = allProducts.Count(p => p.StockQuantity > 0 && p.StockQuantity <= 10),
                OutOfStockProductCount = allProducts.Count(p => p.StockQuantity <= 0),
                TotalImportReceipts = allStockImports.Count,
                TotalImportSubTotal = totalImpSub,
                TotalImportVatAmount = totalImpVat,
                TotalImportWithVat = totalImpSub + totalImpVat,
                TotalExportSubTotal = totalExpSub,
                TotalExportVatAmount = totalExpVat,
                TotalExportWithVat = totalExpSub + totalExpVat
            };

            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ImportStock(int productId, int quantity, decimal? importPrice, string? supplierName, string? note)
        {
            if (quantity <= 0)
            {
                TempData["ErrorMessage"] = "Số lượng nhập kho bắt buộc phải lớn hơn 0.";
                return RedirectToAction(nameof(Warehouse), new { productId });
            }

            var product = await _context.Products
                .Include(p => p.Brand)
                .FirstOrDefaultAsync(p => p.Id == productId);

            if (product == null)
            {
                TempData["ErrorMessage"] = "Không tìm thấy sản phẩm cần nhập kho.";
                return RedirectToAction(nameof(Warehouse));
            }

            var stockBefore = product.StockQuantity;
            product.StockQuantity += quantity;
            var stockAfter = product.StockQuantity;

            var adminName = (HttpContext.Session.GetString("USER_FULLNAME") ?? "Quản trị viên")
                .Replace("(Quản trị viên)", "", StringComparison.OrdinalIgnoreCase)
                .Trim();

            var actualImportPrice = importPrice.HasValue && importPrice.Value > 0
                ? importPrice.Value
                : Math.Round(product.Price * 0.82m / 1000m, 0) * 1000m;
            var actualSupplier = string.IsNullOrWhiteSpace(supplierName)
                ? $"Nhà phân phối chính hãng {product.Brand?.Name ?? "Điện Máy"}"
                : supplierName.Trim();

            var stockImport = new StockImport
            {
                ReceiptCode = "PNK" + DateTime.Now.ToString("yyyyMMddHHmmss"),
                ProductId = product.Id,
                Quantity = quantity,
                ImportPrice = actualImportPrice,
                StockBefore = stockBefore,
                StockAfter = stockAfter,
                SupplierName = actualSupplier,
                Note = string.IsNullOrWhiteSpace(note) ? "Nhập bổ sung hàng hóa vào kho bán lẻ" : note.Trim(),
                CreatedBy = adminName,
                CreatedAt = DateTime.Now
            };

            _context.StockImports.Add(stockImport);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = $"Đã lập phiếu {stockImport.ReceiptCode}: Nhập +{quantity} \"{product.Name}\". Tạm tính: {stockImport.SubTotal:N0} đ | Thuế VAT đầu vào (8%): +{stockImport.VatAmount:N0} đ | Tổng giá trị nhập (gồm VAT): {stockImport.TotalAmountWithVat:N0} đ.";
            return RedirectToAction(nameof(Warehouse));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteProduct(int id)
        {
            var product = await _context.Products.FindAsync(id);
            if (product != null)
            {
                var hasOrder = await _context.OrderDetails.AnyAsync(od => od.ProductId == id);
                if (hasOrder)
                {
                    product.IsActive = false;
                    TempData["SuccessMessage"] = $"Sản phẩm \"{product.Name}\" đã có trong đơn hàng nên được chuyển sang trạng thái Ngừng kinh doanh.";
                }
                else
                {
                    _context.Products.Remove(product);
                    TempData["SuccessMessage"] = $"Đã xóa sản phẩm \"{product.Name}\" khỏi hệ thống.";
                }
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Products));
        }

        // ================= QUẢN LÝ DANH MỤC =================
        public async Task<IActionResult> Categories()
        {
            var categories = await _context.Categories
                .Include(c => c.Products)
                .OrderBy(c => c.DisplayOrder)
                .ToListAsync();
            return View(categories);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SaveCategory(int id, string name, string slug, string iconUrl, string? description, int displayOrder, bool isActive = true)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                TempData["ErrorMessage"] = "Tên danh mục không được để trống.";
                return RedirectToAction(nameof(Categories));
            }

            if (id > 0)
            {
                var cat = await _context.Categories.FindAsync(id);
                if (cat != null)
                {
                    cat.Name = name.Trim();
                    cat.Slug = string.IsNullOrWhiteSpace(slug) ? name.Trim().ToLower().Replace(" ", "-") : slug.Trim();
                    cat.IconUrl = iconUrl.Trim();
                    cat.Description = description?.Trim();
                    cat.DisplayOrder = displayOrder;
                    cat.IsActive = isActive;
                    TempData["SuccessMessage"] = "Cập nhật danh mục thành công.";
                }
            }
            else
            {
                var cat = new Category
                {
                    Name = name.Trim(),
                    Slug = string.IsNullOrWhiteSpace(slug) ? name.Trim().ToLower().Replace(" ", "-") : slug.Trim(),
                    IconUrl = string.IsNullOrWhiteSpace(iconUrl) ? "https://cdnv2.tgdd.vn/pim/cdn/images/202610/Tivi110032.png" : iconUrl.Trim(),
                    Description = description?.Trim(),
                    DisplayOrder = displayOrder,
                    IsActive = isActive
                };
                _context.Categories.Add(cat);
                TempData["SuccessMessage"] = "Thêm danh mục mới thành công.";
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Categories));
        }

        // ================= QUẢN LÝ ĐƠN HÀNG =================
        public async Task<IActionResult> Orders(string? status, string? keyword)
        {
            var query = _context.Orders
                .Include(o => o.OrderDetails)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(status))
            {
                query = query.Where(o => o.OrderStatus == status);
            }

            if (!string.IsNullOrWhiteSpace(keyword))
            {
                var kw = keyword.Trim().ToLower();
                query = query.Where(o =>
                    o.OrderCode.ToLower().Contains(kw) ||
                    o.CustomerName.ToLower().Contains(kw) ||
                    o.CustomerPhone.Contains(kw));
            }

            ViewBag.SelectedStatus = status;
            ViewBag.Keyword = keyword;

            var list = await query.OrderByDescending(o => o.CreatedAt).ToListAsync();
            return View(list);
        }

        public async Task<IActionResult> OrderDetail(int id)
        {
            var order = await _context.Orders
                .Include(o => o.OrderDetails)
                .FirstOrDefaultAsync(o => o.Id == id);

            if (order == null)
            {
                return NotFound();
            }

            return View(order);
        }

        [HttpGet]
        public async Task<IActionResult> PrintInvoice(int id)
        {
            var order = await _context.Orders
                .Include(o => o.OrderDetails)
                .ThenInclude(od => od.Product)
                .FirstOrDefaultAsync(o => o.Id == id);

            if (order == null)
            {
                return NotFound();
            }

            var vm = new PrintInvoiceViewModel
            {
                Order = order,
                AmountInWords = ConvertNumberToVietnameseWords((long)order.TotalAmount)
            };

            return View(vm);
        }

        private static string ConvertNumberToVietnameseWords(long number)
        {
            if (number == 0) return "Không đồng";
            if (number < 0) return "Âm " + ConvertNumberToVietnameseWords(-number);

            string[] units = { "", "nghìn", "triệu", "tỷ", "nghìn tỷ", "triệu tỷ" };
            string[] digits = { "không", "một", "hai", "ba", "bốn", "năm", "sáu", "bảy", "tám", "chín" };

            string sNumber = number.ToString();
            List<string> groups = new List<string>();
            while (sNumber.Length > 0)
            {
                int len = Math.Min(3, sNumber.Length);
                groups.Add(sNumber.Substring(sNumber.Length - len, len));
                sNumber = sNumber.Substring(0, sNumber.Length - len);
            }

            List<string> result = new List<string>();
            for (int i = groups.Count - 1; i >= 0; i--)
            {
                string group = groups[i].PadLeft(3, '0');
                int a = group[0] - '0';
                int b = group[1] - '0';
                int c = group[2] - '0';

                if (a == 0 && b == 0 && c == 0) continue;

                string groupText = "";
                if (a > 0 || (i < groups.Count - 1))
                {
                    groupText += digits[a] + " trăm ";
                }

                if (b == 0 && c > 0 && (a > 0 || i < groups.Count - 1))
                {
                    groupText += "lẻ ";
                }
                else if (b == 1)
                {
                    groupText += "mười ";
                }
                else if (b > 1)
                {
                    groupText += digits[b] + " mươi ";
                }

                if (c == 1 && b > 1)
                {
                    groupText += "mốt";
                }
                else if (c == 5 && b > 0)
                {
                    groupText += "lăm";
                }
                else if (c > 0)
                {
                    groupText += digits[c];
                }

                groupText = groupText.Trim() + " " + units[i];
                result.Add(groupText.Trim());
            }

            string finalStr = string.Join(" ", result).Trim();
            if (string.IsNullOrWhiteSpace(finalStr)) return "Không đồng";

            finalStr = char.ToUpper(finalStr[0]) + finalStr.Substring(1) + " đồng chẵn.";
            return finalStr;
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateOrderStatus(int id, string orderStatus, string paymentStatus)
        {
            var order = await _context.Orders
                .Include(o => o.OrderDetails)
                .FirstOrDefaultAsync(o => o.Id == id);

            if (order == null)
            {
                return NotFound();
            }

            var oldStatus = order.OrderStatus;

            // Hoàn kho & hoàn lượt dùng Voucher nếu chuyển từ đơn hiệu lực sang Đã hủy
            if (oldStatus != "Đã hủy" && orderStatus == "Đã hủy")
            {
                foreach (var detail in order.OrderDetails)
                {
                    var product = await _context.Products.FindAsync(detail.ProductId);
                    if (product != null)
                    {
                        product.StockQuantity += detail.Quantity;
                        product.SoldCount = Math.Max(0, product.SoldCount - detail.Quantity);
                    }
                }

                if (!string.IsNullOrWhiteSpace(order.CouponCode))
                {
                    var coupon = await _context.Coupons.FirstOrDefaultAsync(c => c.Code == order.CouponCode);
                    if (coupon != null)
                    {
                        coupon.UsedCount = Math.Max(0, coupon.UsedCount - 1);
                    }
                }
            }
            // Trừ kho & cộng lại lượt dùng Voucher nếu khôi phục đơn từ Đã hủy sang trạng thái hiệu lực
            else if (oldStatus == "Đã hủy" && orderStatus != "Đã hủy")
            {
                foreach (var detail in order.OrderDetails)
                {
                    var product = await _context.Products.FindAsync(detail.ProductId);
                    if (product == null || product.StockQuantity < detail.Quantity)
                    {
                        TempData["ErrorMessage"] = $"Không đủ tồn kho cho sản phẩm \"{detail.ProductName}\" (Tồn: {product?.StockQuantity ?? 0}, Cần: {detail.Quantity}).";
                        return RedirectToAction(nameof(OrderDetail), new { id });
                    }
                }

                foreach (var detail in order.OrderDetails)
                {
                    var product = await _context.Products.FindAsync(detail.ProductId);
                    if (product != null)
                    {
                        product.StockQuantity -= detail.Quantity;
                        product.SoldCount += detail.Quantity;
                    }
                }

                if (!string.IsNullOrWhiteSpace(order.CouponCode))
                {
                    var coupon = await _context.Coupons.FirstOrDefaultAsync(c => c.Code == order.CouponCode);
                    if (coupon != null)
                    {
                        coupon.UsedCount += 1;
                    }
                }
            }

            order.OrderStatus = orderStatus;
            order.PaymentStatus = paymentStatus;
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = $"Đã cập nhật trạng thái đơn hàng {order.OrderCode}.";
            return RedirectToAction(nameof(OrderDetail), new { id });
        }

        // ================= QUẢN LÝ NGƯỜI DÙNG =================
        public async Task<IActionResult> Users()
        {
            var users = await _context.Users
                .Include(u => u.Orders)
                .OrderByDescending(u => u.Role)
                .ThenByDescending(u => u.CreatedAt)
                .ToListAsync();
            return View(users);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleUserRole(int id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user != null)
            {
                if (user.Username == "admin")
                {
                    TempData["ErrorMessage"] = "Không thể hạ quyền của tài khoản Quản trị viên gốc.";
                    return RedirectToAction(nameof(Users));
                }
                user.Role = user.Role == "Admin" ? "Customer" : "Admin";
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = $"Đã chuyển quyền của {user.FullName} thành {user.Role}.";
            }
            return RedirectToAction(nameof(Users));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleUserStatus(int id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user != null && user.Username != "admin")
            {
                user.IsActive = !user.IsActive;
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = $"Đã cập nhật trạng thái tài khoản {user.Username}.";
            }
            return RedirectToAction(nameof(Users));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteUser(int id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null)
            {
                TempData["ErrorMessage"] = "Không tìm thấy người dùng cần xóa.";
                return RedirectToAction(nameof(Users));
            }

            if (string.Equals(user.Username, "admin", StringComparison.OrdinalIgnoreCase))
            {
                TempData["ErrorMessage"] = "Không thể xóa tài khoản Quản trị viên gốc (admin).";
                return RedirectToAction(nameof(Users));
            }

            var currentUserId = HttpContext.Session.GetInt32("USER_ID");
            if (currentUserId.HasValue && user.Id == currentUserId.Value)
            {
                TempData["ErrorMessage"] = "Bạn không thể tự xóa tài khoản đang đăng nhập hiện tại.";
                return RedirectToAction(nameof(Users));
            }

            var userOrders = await _context.Orders.Where(o => o.UserId == id).ToListAsync();
            foreach (var ord in userOrders)
            {
                ord.UserId = null;
            }

            var userReviews = await _context.ProductReviews.Where(r => r.UserId == id).ToListAsync();
            foreach (var rev in userReviews)
            {
                rev.UserId = null;
            }

            var deletedName = user.FullName;
            var deletedUsername = user.Username;

            _context.Users.Remove(user);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = $"Đã xóa tài khoản \"{deletedName}\" (@{deletedUsername}) thành công.";
            return RedirectToAction(nameof(Users));
        }

        // ================= QUẢN LÝ ĐÁNH GIÁ =================
        public async Task<IActionResult> Reviews()
        {
            var reviews = await _context.ProductReviews
                .Include(r => r.Product)
                .OrderByDescending(r => r.CreatedAt)
                .ToListAsync();
            return View(reviews);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteReview(int id)
        {
            var review = await _context.ProductReviews.FindAsync(id);
            if (review != null)
            {
                _context.ProductReviews.Remove(review);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Đã xóa đánh giá sản phẩm.";
            }
            return RedirectToAction(nameof(Reviews));
        }

        // ================= QUẢN LÝ THƯƠNG HIỆU HÃNG =================
        [HttpGet]
        public async Task<IActionResult> Brands()
        {
            var brands = await _context.Brands
                .Include(b => b.Products)
                .OrderBy(b => b.Name)
                .ToListAsync();
            return View(brands);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SaveBrand(Brand model)
        {
            if (string.IsNullOrWhiteSpace(model.Name))
            {
                TempData["ErrorMessage"] = "Vui lòng nhập tên thương hiệu.";
                return RedirectToAction(nameof(Brands));
            }

            if (string.IsNullOrWhiteSpace(model.Slug))
            {
                model.Slug = model.Name.ToLower().Replace(" ", "-");
            }

            if (model.Id > 0)
            {
                var existing = await _context.Brands.FindAsync(model.Id);
                if (existing != null)
                {
                    existing.Name = model.Name.Trim();
                    existing.Slug = model.Slug.Trim();
                    existing.LogoUrl = model.LogoUrl?.Trim() ?? "";
                    existing.Origin = string.IsNullOrWhiteSpace(model.Origin) ? "Chính hãng" : model.Origin.Trim();
                    TempData["SuccessMessage"] = $"Đã cập nhật thương hiệu \"{existing.Name}\".";
                }
            }
            else
            {
                var newBrand = new Brand
                {
                    Name = model.Name.Trim(),
                    Slug = model.Slug.Trim(),
                    LogoUrl = model.LogoUrl?.Trim() ?? "https://cdn.tgdd.vn/2026/11/timerseo/1751098000005-may-lanh-tron-20261105-600x600.png",
                    Origin = string.IsNullOrWhiteSpace(model.Origin) ? "Chính hãng" : model.Origin.Trim()
                };
                _context.Brands.Add(newBrand);
                TempData["SuccessMessage"] = $"Đã thêm thương hiệu mới \"{newBrand.Name}\".";
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Brands));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteBrand(int id)
        {
            var brand = await _context.Brands
                .Include(b => b.Products)
                .FirstOrDefaultAsync(b => b.Id == id);

            if (brand != null)
            {
                if (brand.Products.Any())
                {
                    TempData["ErrorMessage"] = $"Không thể xóa thương hiệu \"{brand.Name}\" vì đang có {brand.Products.Count} sản phẩm trực thuộc.";
                }
                else
                {
                    _context.Brands.Remove(brand);
                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = $"Đã xóa thương hiệu \"{brand.Name}\".";
                }
            }
            return RedirectToAction(nameof(Brands));
        }

        // ================= QUẢN LÝ MÃ GIẢM GIÁ (COUPONS / VOUCHERS) =================
        [HttpGet]
        public async Task<IActionResult> Coupons()
        {
            var coupons = await _context.Coupons
                .OrderByDescending(c => c.IsActive)
                .ThenByDescending(c => c.StartDate)
                .ToListAsync();
            return View(coupons);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SaveCoupon(Coupon model, bool isActive = false)
        {
            if (string.IsNullOrWhiteSpace(model.Code) || string.IsNullOrWhiteSpace(model.Description))
            {
                TempData["ErrorMessage"] = "Vui lòng nhập đầy đủ mã và mô tả cho Voucher.";
                return RedirectToAction(nameof(Coupons));
            }

            model.Code = model.Code.Trim().ToUpper();
            model.Description = model.Description.Trim();
            model.IsActive = isActive;
            model.MinOrderAmount = Math.Max(0, model.MinOrderAmount);
            model.UsageLimit = Math.Max(1, model.UsageLimit);

            if (model.DiscountValue <= 0)
            {
                TempData["ErrorMessage"] = "Giá trị giảm giá bắt buộc phải lớn hơn 0.";
                return RedirectToAction(nameof(Coupons));
            }

            if (model.DiscountType == "Percentage")
            {
                if (model.DiscountValue > 100)
                {
                    TempData["ErrorMessage"] = "Tỷ lệ giảm giá theo phần trăm (%) không được vượt quá 100%.";
                    return RedirectToAction(nameof(Coupons));
                }
                model.MaxDiscountAmount = (model.MaxDiscountAmount.HasValue && model.MaxDiscountAmount.Value > 0)
                    ? model.MaxDiscountAmount.Value
                    : null;
            }
            else
            {
                model.DiscountType = "FixedAmount";
                model.MaxDiscountAmount = null;
            }

            if (model.EndDate.Date < model.StartDate.Date)
            {
                TempData["ErrorMessage"] = "Ngày kết thúc của Voucher không được nhỏ hơn Ngày bắt đầu.";
                return RedirectToAction(nameof(Coupons));
            }

            model.StartDate = model.StartDate.Date;
            model.EndDate = model.EndDate.Date.AddHours(23).AddMinutes(59).AddSeconds(59);

            var isDuplicate = await _context.Coupons.AnyAsync(c => c.Code == model.Code && c.Id != model.Id);
            if (isDuplicate)
            {
                TempData["ErrorMessage"] = $"Mã ưu đãi \"{model.Code}\" đã tồn tại trong hệ thống.";
                return RedirectToAction(nameof(Coupons));
            }

            if (model.Id > 0)
            {
                var existing = await _context.Coupons.FindAsync(model.Id);
                if (existing != null)
                {
                    existing.Code = model.Code;
                    existing.Description = model.Description;
                    existing.DiscountType = model.DiscountType;
                    existing.DiscountValue = model.DiscountValue;
                    existing.MinOrderAmount = model.MinOrderAmount;
                    existing.MaxDiscountAmount = model.MaxDiscountAmount;
                    existing.UsageLimit = model.UsageLimit;
                    existing.StartDate = model.StartDate;
                    existing.EndDate = model.EndDate;
                    existing.IsActive = model.IsActive;
                    TempData["SuccessMessage"] = $"Đã cập nhật mã ưu đãi \"{existing.Code}\" thành công.";
                }
            }
            else
            {
                _context.Coupons.Add(model);
                TempData["SuccessMessage"] = $"Đã tạo mã ưu đãi mới \"{model.Code}\" thành công.";
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Coupons));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleCouponStatus(int id)
        {
            var coupon = await _context.Coupons.FindAsync(id);
            if (coupon != null)
            {
                coupon.IsActive = !coupon.IsActive;
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = $"Đã cập nhật trạng thái mã \"{coupon.Code}\": {(coupon.IsActive ? "Đang áp dụng" : "Tạm dừng")}.";
            }
            return RedirectToAction(nameof(Coupons));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteCoupon(int id)
        {
            var coupon = await _context.Coupons.FindAsync(id);
            if (coupon != null)
            {
                _context.Coupons.Remove(coupon);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = $"Đã xóa mã ưu đãi \"{coupon.Code}\".";
            }
            return RedirectToAction(nameof(Coupons));
        }

        // ================= BÁO CÁO THỐNG KÊ DOANH THU & XUẤT PDF =================
        [HttpGet]
        public async Task<IActionResult> RevenueReport(DateTime? fromDate, DateTime? toDate, string? quickFilter, int? categoryId, string? status)
        {
            var vm = await BuildRevenueReportViewModelAsync(fromDate, toDate, quickFilter, categoryId, status);
            return View(vm);
        }

        [HttpGet]
        public async Task<IActionResult> ExportRevenuePdf(DateTime? fromDate, DateTime? toDate, string? quickFilter, int? categoryId, string? status)
        {
            var vm = await BuildRevenueReportViewModelAsync(fromDate, toDate, quickFilter, categoryId, status);
            return View(vm);
        }

        private async Task<RevenueReportViewModel> BuildRevenueReportViewModelAsync(
            DateTime? fromDate,
            DateTime? toDate,
            string? quickFilter,
            int? categoryId,
            string? status)
        {
            var now = DateTime.Now;
            DateTime start;
            DateTime end;

            var qf = string.IsNullOrWhiteSpace(quickFilter) ? "30days" : quickFilter.Trim().ToLower();

            switch (qf)
            {
                case "today":
                    start = now.Date;
                    end = now.Date.AddDays(1).AddTicks(-1);
                    break;
                case "7days":
                    start = now.Date.AddDays(-6);
                    end = now.Date.AddDays(1).AddTicks(-1);
                    break;
                case "thismonth":
                    start = new DateTime(now.Year, now.Month, 1);
                    end = now.Date.AddDays(1).AddTicks(-1);
                    break;
                case "lastmonth":
                    var lastMonth = now.AddMonths(-1);
                    start = new DateTime(lastMonth.Year, lastMonth.Month, 1);
                    end = new DateTime(now.Year, now.Month, 1).AddTicks(-1);
                    break;
                case "thisyear":
                    start = new DateTime(now.Year, 1, 1);
                    end = now.Date.AddDays(1).AddTicks(-1);
                    break;
                case "all":
                    start = new DateTime(2026, 1, 1);
                    end = now.Date.AddDays(1).AddTicks(-1);
                    break;
                case "custom":
                    start = fromDate ?? now.Date.AddDays(-29);
                    end = (toDate ?? now).Date.AddDays(1).AddTicks(-1);
                    break;
                case "30days":
                default:
                    qf = "30days";
                    start = now.Date.AddDays(-29);
                    end = now.Date.AddDays(1).AddTicks(-1);
                    break;
            }

            if (fromDate.HasValue && toDate.HasValue && qf != "today" && qf != "7days" && qf != "thismonth" && qf != "lastmonth" && qf != "thisyear" && qf != "all")
            {
                start = fromDate.Value.Date;
                end = toDate.Value.Date.AddDays(1).AddTicks(-1);
                qf = "custom";
            }

            var query = _context.Orders
                .Include(o => o.OrderDetails)
                .ThenInclude(od => od.Product)
                .ThenInclude(p => p!.Category)
                .Where(o => o.CreatedAt >= start && o.CreatedAt <= end)
                .AsQueryable();

            var orderStatusFilter = string.IsNullOrWhiteSpace(status) ? "valid" : status.Trim();
            if (orderStatusFilter == "valid")
            {
                query = query.Where(o => o.OrderStatus != "Đã hủy");
            }
            else if (orderStatusFilter == "completed")
            {
                query = query.Where(o => o.OrderStatus == "Hoàn thành");
            }
            else if (orderStatusFilter != "all")
            {
                query = query.Where(o => o.OrderStatus == orderStatusFilter);
            }

            var rawOrders = await query.OrderByDescending(o => o.CreatedAt).ToListAsync();

            var filteredOrders = rawOrders;
            if (categoryId.HasValue && categoryId.Value > 0)
            {
                filteredOrders = rawOrders
                    .Where(o => o.OrderDetails.Any(od => od.Product != null && od.Product.CategoryId == categoryId.Value))
                    .ToList();
            }

            var productIds = filteredOrders.SelectMany(o => o.OrderDetails).Select(od => od.ProductId).Distinct().ToList();
            var stockImports = await _context.StockImports
                .Where(si => productIds.Contains(si.ProductId))
                .ToListAsync();

            var productCostMap = new Dictionary<int, decimal>();
            foreach (var pid in productIds)
            {
                var imports = stockImports.Where(si => si.ProductId == pid).ToList();
                if (imports.Any())
                {
                    productCostMap[pid] = Math.Round(imports.Average(si => si.ImportPrice), 0);
                }
            }

            var grossRevenue = filteredOrders.Sum(o => o.SubTotal);
            var totalDiscount = filteredOrders.Sum(o => o.DiscountAmount);
            var netRevenue = filteredOrders.Sum(o => Math.Max(0, o.SubTotal - o.DiscountAmount));
            var totalOutputVat = filteredOrders.Sum(o => o.VatAmount > 0 ? o.VatAmount : Math.Round(Math.Max(0, o.SubTotal - o.DiscountAmount) * 0.08m, 0));
            var totalRevenueWithVat = netRevenue + totalOutputVat;
            var totalOrders = filteredOrders.Count;

            var periodStockImports = await _context.StockImports
                .AsNoTracking()
                .Where(si => si.CreatedAt >= start && si.CreatedAt <= end)
                .ToListAsync();
            var totalStockImportSubTotal = periodStockImports.Sum(si => si.ImportPrice * si.Quantity);
            var totalInputVat = periodStockImports.Sum(si => Math.Round(si.ImportPrice * si.Quantity * 0.08m, 0));
            var totalStockImportWithVat = totalStockImportSubTotal + totalInputVat;

            decimal totalCost = 0;
            int totalSoldProducts = 0;

            foreach (var o in filteredOrders)
            {
                foreach (var od in o.OrderDetails)
                {
                    totalSoldProducts += od.Quantity;
                    var costPerUnit = productCostMap.ContainsKey(od.ProductId) && productCostMap[od.ProductId] > 0
                        ? productCostMap[od.ProductId]
                        : Math.Round(od.UnitPrice * 0.82m, 0);

                    totalCost += costPerUnit * od.Quantity;
                }
            }

            var estimatedProfit = Math.Max(0, netRevenue - totalCost);
            var profitMargin = netRevenue > 0 ? Math.Round((double)(estimatedProfit / netRevenue) * 100, 1) : 0;
            var aov = totalOrders > 0 ? Math.Round(netRevenue / totalOrders, 0) : 0;

            var dailyGroups = filteredOrders
                .GroupBy(o => o.CreatedAt.Date)
                .OrderBy(g => g.Key)
                .ToList();

            var dailyRevenues = new List<DailyRevenueItem>();
            foreach (var g in dailyGroups)
            {
                var dayGross = g.Sum(o => o.SubTotal);
                var dayDisc = g.Sum(o => o.DiscountAmount);
                var dayNet = g.Sum(o => Math.Max(0, o.SubTotal - o.DiscountAmount));
                var dayVat = g.Sum(o => o.VatAmount > 0 ? o.VatAmount : Math.Round(Math.Max(0, o.SubTotal - o.DiscountAmount) * 0.08m, 0));
                var dayTotalWithVat = dayNet + dayVat;
                var dayOrders = g.Count();
                var dayProds = g.SelectMany(o => o.OrderDetails).Sum(od => od.Quantity);

                decimal dayCost = 0;
                foreach (var o in g)
                {
                    foreach (var od in o.OrderDetails)
                    {
                        var cpu = productCostMap.ContainsKey(od.ProductId) && productCostMap[od.ProductId] > 0
                            ? productCostMap[od.ProductId]
                            : Math.Round(od.UnitPrice * 0.82m, 0);
                        dayCost += cpu * od.Quantity;
                    }
                }

                dailyRevenues.Add(new DailyRevenueItem
                {
                    Date = g.Key,
                    DateString = g.Key.ToString("dd/MM"),
                    FullDateString = g.Key.ToString("dd/MM/yyyy"),
                    OrdersCount = dayOrders,
                    ProductsCount = dayProds,
                    GrossRevenue = dayGross,
                    DiscountAmount = dayDisc,
                    NetRevenue = dayNet,
                    VatAmount = dayVat,
                    TotalWithVat = dayTotalWithVat,
                    EstimatedProfit = Math.Max(0, dayNet - dayCost)
                });
            }

            var categoryMap = new Dictionary<int, CategoryRevenueItem>();
            foreach (var o in filteredOrders)
            {
                foreach (var od in o.OrderDetails)
                {
                    var catId = od.Product?.CategoryId ?? 0;
                    var catName = od.Product?.Category?.Name ?? "Thiết bị khác";
                    var icon = od.Product?.Category?.IconUrl ?? "";

                    if (!categoryMap.ContainsKey(catId))
                    {
                        categoryMap[catId] = new CategoryRevenueItem
                        {
                            CategoryId = catId,
                            CategoryName = catName,
                            IconUrl = icon,
                            TotalQuantity = 0,
                            TotalRevenue = 0
                        };
                    }

                    categoryMap[catId].TotalQuantity += od.Quantity;
                    categoryMap[catId].TotalRevenue += od.UnitPrice * od.Quantity;
                }
            }

            var categoryRevenues = categoryMap.Values
                .OrderByDescending(c => c.TotalRevenue)
                .ToList();

            var totalCatRev = categoryRevenues.Sum(c => c.TotalRevenue);
            foreach (var cr in categoryRevenues)
            {
                cr.Percentage = totalCatRev > 0 ? Math.Round((double)(cr.TotalRevenue / totalCatRev) * 100, 1) : 0;
            }

            var productGroupMap = new Dictionary<int, TopProductRevenueItem>();
            foreach (var o in filteredOrders)
            {
                foreach (var od in o.OrderDetails)
                {
                    if (!productGroupMap.ContainsKey(od.ProductId))
                    {
                        productGroupMap[od.ProductId] = new TopProductRevenueItem
                        {
                            ProductId = od.ProductId,
                            ProductCode = od.Product?.Code ?? $"SP-{od.ProductId}",
                            ProductName = od.ProductName,
                            ImageUrl = od.ProductImage,
                            CategoryName = od.Product?.Category?.Name ?? "",
                            UnitPrice = od.UnitPrice,
                            QuantitySold = 0,
                            TotalRevenue = 0
                        };
                    }

                    productGroupMap[od.ProductId].QuantitySold += od.Quantity;
                    productGroupMap[od.ProductId].TotalRevenue += od.UnitPrice * od.Quantity;
                }
            }

            var topProducts = productGroupMap.Values
                .OrderByDescending(p => p.TotalRevenue)
                .Take(10)
                .ToList();

            foreach (var tp in topProducts)
            {
                tp.RevenuePercentage = netRevenue > 0 ? Math.Round((double)(tp.TotalRevenue / netRevenue) * 100, 1) : 0;
            }

            var adminName = (HttpContext.Session.GetString("USER_FULLNAME") ?? "Quản trị viên")
                .Replace("(Quản trị viên)", "", StringComparison.OrdinalIgnoreCase)
                .Trim();

            var vm = new RevenueReportViewModel
            {
                FromDate = start,
                ToDate = end,
                QuickFilter = qf,
                SelectedCategoryId = categoryId,
                SelectedStatus = orderStatusFilter,
                GrossRevenue = grossRevenue,
                TotalDiscount = totalDiscount,
                NetRevenue = netRevenue,
                TotalOutputVat = totalOutputVat,
                TotalRevenueWithVat = totalRevenueWithVat,
                TotalStockImportSubTotal = totalStockImportSubTotal,
                TotalInputVat = totalInputVat,
                TotalStockImportWithVat = totalStockImportWithVat,
                TotalOrders = totalOrders,
                TotalSoldProducts = totalSoldProducts,
                AverageOrderValue = aov,
                EstimatedCost = totalCost,
                EstimatedGrossProfit = estimatedProfit,
                ProfitMarginPercent = profitMargin,
                DailyRevenues = dailyRevenues,
                CategoryRevenues = categoryRevenues,
                TopProducts = topProducts,
                Orders = filteredOrders.Take(25).ToList(),
                Categories = await _context.Categories.OrderBy(c => c.DisplayOrder).ToListAsync(),
                ReportCode = "BCDT-" + DateTime.Now.ToString("yyyyMMdd-HHmm"),
                ExportedAt = DateTime.Now,
                ExportedBy = adminName
            };

            return vm;
        }

        // ================= XUẤT HÓA ĐƠN NHẬP KHO DẠNG PDF (A4) =================
        [HttpGet]
        public async Task<IActionResult> PrintStockImport(int id)
        {
            var stockImport = await _context.StockImports
                .Include(si => si.Product)
                .ThenInclude(p => p!.Category)
                .Include(si => si.Product)
                .ThenInclude(p => p!.Brand)
                .FirstOrDefaultAsync(si => si.Id == id);

            if (stockImport == null)
            {
                return NotFound();
            }

            var vm = new PrintStockImportViewModel
            {
                StockImport = stockImport
            };
            vm.AmountInWords = ConvertNumberToVietnameseWords((long)Math.Round(vm.TotalAmount, 0));

            return View(vm);
        }

        // ================= QUẢN LÝ BANNER QUẢNG CÁO TRANG CHỦ =================
        [HttpGet]
        public async Task<IActionResult> Banners(int? editId)
        {
            var banners = await _context.Banners
                .OrderBy(b => b.Position)
                .ThenBy(b => b.DisplayOrder)
                .ThenByDescending(b => b.Id)
                .ToListAsync();

            Banner? editingBanner = null;
            if (editId.HasValue)
            {
                editingBanner = banners.FirstOrDefault(b => b.Id == editId.Value);
            }

            ViewBag.EditingBanner = editingBanner;
            return View(banners);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SaveBanner(Banner model)
        {
            if (string.IsNullOrWhiteSpace(model.Title) || string.IsNullOrWhiteSpace(model.ImageUrl))
            {
                TempData["ErrorMessage"] = "Vui lòng nhập đầy đủ Tiêu đề và Đường dẫn hình ảnh Banner.";
                return RedirectToAction(nameof(Banners));
            }

            if (string.IsNullOrWhiteSpace(model.LinkUrl))
            {
                model.LinkUrl = "/Product";
            }

            if (model.Id > 0)
            {
                var existing = await _context.Banners.FindAsync(model.Id);
                if (existing != null)
                {
                    existing.Title = model.Title.Trim();
                    existing.Subtitle = model.Subtitle?.Trim();
                    existing.ImageUrl = model.ImageUrl.Trim();
                    existing.LinkUrl = model.LinkUrl.Trim();
                    existing.Position = model.Position;
                    existing.DisplayOrder = model.DisplayOrder;
                    existing.IsActive = model.IsActive;
                    TempData["SuccessMessage"] = $"Đã cập nhật banner quảng cáo \"{existing.Title}\".";
                }
            }
            else
            {
                model.Title = model.Title.Trim();
                model.Subtitle = model.Subtitle?.Trim();
                model.ImageUrl = model.ImageUrl.Trim();
                model.LinkUrl = model.LinkUrl.Trim();
                model.CreatedAt = DateTime.Now;
                _context.Banners.Add(model);
                TempData["SuccessMessage"] = $"Đã thêm banner quảng cáo mới \"{model.Title}\".";
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Banners));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleBanner(int id)
        {
            var banner = await _context.Banners.FindAsync(id);
            if (banner != null)
            {
                banner.IsActive = !banner.IsActive;
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = $"Đã chuyển trạng thái banner \"{banner.Title}\" sang {(banner.IsActive ? "Hiển thị" : "Đang ẩn")}.";
            }
            return RedirectToAction(nameof(Banners));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteBanner(int id)
        {
            var banner = await _context.Banners.FindAsync(id);
            if (banner != null)
            {
                _context.Banners.Remove(banner);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = $"Đã xóa banner quảng cáo \"{banner.Title}\".";
            }
            return RedirectToAction(nameof(Banners));
        }

        // ================= CẤU HÌNH NGÂN HÀNG & VIETQR =================
        [HttpGet]
        public IActionResult BankConfig()
        {
            var accounts = BankConfigStore.GetAll();
            return View(accounts);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult SaveBankConfig(BankAccountConfig model)
        {
            if (string.IsNullOrWhiteSpace(model.BankCode) ||
                string.IsNullOrWhiteSpace(model.AccountNumber) ||
                string.IsNullOrWhiteSpace(model.AccountHolder))
            {
                TempData["ErrorMessage"] = "Vui lòng nhập đầy đủ Ngân hàng, Số tài khoản và Tên chủ tài khoản.";
                return RedirectToAction(nameof(BankConfig));
            }

            var isUpdate = model.Id > 0;
            BankConfigStore.Save(model);

            TempData["SuccessMessage"] = isUpdate
                ? $"Đã cập nhật tài khoản ngân hàng {model.BankCode} - {model.AccountNumber}."
                : $"Đã thêm tài khoản ngân hàng mới {model.BankCode} - {model.AccountNumber}.";

            return RedirectToAction(nameof(BankConfig));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult SetDefaultBank(int id)
        {
            BankConfigStore.SetDefault(id);
            TempData["SuccessMessage"] = "Đã đặt tài khoản ngân hàng làm mặc định tạo mã VietQR thanh toán.";
            return RedirectToAction(nameof(BankConfig));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult ToggleBankStatus(int id)
        {
            BankConfigStore.ToggleActive(id);
            TempData["SuccessMessage"] = "Đã cập nhật trạng thái hiển thị của tài khoản ngân hàng.";
            return RedirectToAction(nameof(BankConfig));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteBankConfig(int id)
        {
            if (BankConfigStore.Delete(id, out var error))
            {
                TempData["SuccessMessage"] = "Đã xóa tài khoản ngân hàng khỏi cấu hình.";
            }
            else
            {
                TempData["ErrorMessage"] = error;
            }
            return RedirectToAction(nameof(BankConfig));
        }
    }
}
