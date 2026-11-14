using System.Collections.Concurrent;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebsiteDienMay.Data;
using WebsiteDienMay.Helpers;
using WebsiteDienMay.Models;

namespace WebsiteDienMay.Controllers
{
    public class CartController : Controller
    {
        private readonly ApplicationDbContext _context;
        private const string CartSessionKey = "CART_SESSION";
        private const string CouponSessionKey = "COUPON_SESSION";
        private static readonly HttpClient _httpClient = new HttpClient { Timeout = TimeSpan.FromSeconds(10) };
        private static string? _cachedProvincesJson;
        private static readonly ConcurrentDictionary<int, string> _cachedWardsJson = new();

        public CartController(ApplicationDbContext context)
        {
            _context = context;
        }

        private List<CartItemViewModel> GetCartItems()
        {
            return HttpContext.Session.GetObjectFromJson<List<CartItemViewModel>>(CartSessionKey) ?? new List<CartItemViewModel>();
        }

        private void SaveCartItems(List<CartItemViewModel> items)
        {
            HttpContext.Session.SetObjectAsJson(CartSessionKey, items);
        }

        private decimal CalculateDiscount(decimal subTotal, string? couponCode, out Coupon? matchedCoupon)
        {
            matchedCoupon = null;
            if (string.IsNullOrWhiteSpace(couponCode) || subTotal <= 0) return 0;
            var code = couponCode.Trim().ToUpper();

            var coupon = _context.Coupons.AsNoTracking().FirstOrDefault(c => c.Code == code && c.IsActive);
            if (coupon == null) return 0;

            var today = DateTime.Now.Date;
            if (today < coupon.StartDate.Date || today > coupon.EndDate.Date) return 0;
            if (coupon.UsedCount >= coupon.UsageLimit) return 0;
            if (subTotal < coupon.MinOrderAmount) return 0;

            matchedCoupon = coupon;

            if (coupon.DiscountType == "Percentage")
            {
                var discount = Math.Round(subTotal * (coupon.DiscountValue / 100m), 0);
                if (coupon.MaxDiscountAmount.HasValue && coupon.MaxDiscountAmount.Value > 0)
                {
                    discount = Math.Min(discount, coupon.MaxDiscountAmount.Value);
                }
                return Math.Min(subTotal, discount);
            }
            else
            {
                return Math.Min(subTotal, coupon.DiscountValue);
            }
        }

        private CartViewModel BuildCartViewModel()
        {
            var items = GetCartItems();
            var couponCode = HttpContext.Session.GetString(CouponSessionKey);
            var subTotal = items.Sum(x => x.Total);
            var discount = CalculateDiscount(subTotal, couponCode, out var matchedCoupon);

            if (!string.IsNullOrEmpty(couponCode) && (discount <= 0 || matchedCoupon == null))
            {
                HttpContext.Session.Remove(CouponSessionKey);
                couponCode = null;
                discount = 0;
            }

            var today = DateTime.Now.Date;
            var availableCoupons = _context.Coupons
                .AsNoTracking()
                .Where(c => c.IsActive && c.StartDate.Date <= today && c.EndDate.Date >= today && c.UsedCount < c.UsageLimit)
                .OrderBy(c => c.MinOrderAmount)
                .ThenByDescending(c => c.DiscountValue)
                .ToList();

            return new CartViewModel
            {
                Items = items,
                AvailableCoupons = availableCoupons,
                CouponCode = couponCode,
                AppliedCouponDescription = matchedCoupon?.Description,
                DiscountAmount = discount
            };
        }

        public IActionResult Index()
        {
            var vm = BuildCartViewModel();
            return View(vm);
        }

        [HttpPost]
        public async Task<IActionResult> AddToCart(int productId, int quantity = 1, bool buyNow = false, string? selectedVariant = null, string? defaultPaymentMethod = null)
        {
            var product = await _context.Products.FirstOrDefaultAsync(p => p.Id == productId && p.IsActive);
            if (product == null)
            {
                return NotFound();
            }

            if (product.StockQuantity <= 0)
            {
                TempData["ErrorMessage"] = $"Sản phẩm \"{product.Name}\" hiện đã hết hàng trong kho.";
                return RedirectToAction("Detail", "Product", new { id = productId });
            }

            // Phân giải biến thể sản phẩm (nếu có)
            var variants = ProductVariantItem.ParseVariants(product.ProductVariants, product.Price, product.OriginalPrice, product.ImageUrl);
            string? resolvedVariant = null;
            decimal resolvedPrice = product.Price;
            decimal resolvedOrigPrice = product.OriginalPrice;
            string resolvedImageUrl = product.ImageUrl;

            if (variants.Any())
            {
                var matched = !string.IsNullOrWhiteSpace(selectedVariant)
                    ? variants.FirstOrDefault(v => string.Equals(v.VariantName, selectedVariant.Trim(), StringComparison.OrdinalIgnoreCase))
                    : null;

                matched ??= variants.First();
                resolvedVariant = matched.VariantName;
                resolvedPrice = matched.Price;
                resolvedOrigPrice = matched.OriginalPrice;
                if (!string.IsNullOrWhiteSpace(matched.ImageUrl))
                {
                    resolvedImageUrl = matched.ImageUrl;
                }
            }

            var reqQty = Math.Max(1, quantity);
            var cart = GetCartItems();
            var totalProdQtyInCart = cart.Where(x => x.ProductId == productId).Sum(x => x.Quantity);
            var existing = cart.FirstOrDefault(x =>
                x.ProductId == productId &&
                string.Equals(x.SelectedVariant ?? "", resolvedVariant ?? "", StringComparison.OrdinalIgnoreCase));

            if (existing != null)
            {
                var otherVariantsQty = totalProdQtyInCart - existing.Quantity;
                var maxForThisVariant = Math.Max(0, product.StockQuantity - otherVariantsQty);
                var newQty = existing.Quantity + reqQty;
                if (newQty > maxForThisVariant)
                {
                    existing.Quantity = maxForThisVariant;
                    SaveCartItems(cart);
                    TempData["ErrorMessage"] = $"Kho hiện chỉ còn {product.StockQuantity} sản phẩm \"{product.Name}\".";
                    return RedirectToAction(nameof(Index));
                }
                existing.Quantity = newQty;
                existing.UnitPrice = resolvedPrice;
                existing.OriginalPrice = resolvedOrigPrice;
                existing.ImageUrl = resolvedImageUrl;
            }
            else
            {
                var remainingStock = Math.Max(0, product.StockQuantity - totalProdQtyInCart);
                if (remainingStock <= 0)
                {
                    TempData["ErrorMessage"] = $"Bạn đã thêm tối đa số lượng tồn kho ({product.StockQuantity}) của \"{product.Name}\" vào giỏ hàng.";
                    return RedirectToAction(nameof(Index));
                }

                var allowedQty = Math.Min(reqQty, remainingStock);
                cart.Add(new CartItemViewModel
                {
                    ProductId = product.Id,
                    ProductName = product.Name,
                    ProductCode = product.Code,
                    SelectedVariant = resolvedVariant,
                    ImageUrl = resolvedImageUrl,
                    UnitPrice = resolvedPrice,
                    OriginalPrice = resolvedOrigPrice,
                    Quantity = allowedQty
                });

                if (reqQty > remainingStock)
                {
                    SaveCartItems(cart);
                    TempData["ErrorMessage"] = $"Kho hiện chỉ còn {product.StockQuantity} sản phẩm \"{product.Name}\".";
                    return RedirectToAction(nameof(Index));
                }
            }

            SaveCartItems(cart);

            if (!string.IsNullOrWhiteSpace(defaultPaymentMethod))
            {
                TempData["DefaultPaymentMethod"] = defaultPaymentMethod;
            }

            if (buyNow)
            {
                if (!HttpContext.Session.GetInt32("USER_ID").HasValue)
                {
                    TempData["ErrorMessage"] = "Vui lòng đăng nhập tài khoản khách hàng để tiến hành thanh toán đơn hàng.";
                    return RedirectToAction("Login", "Account", new { returnUrl = "/Cart/Checkout" });
                }
                return RedirectToAction(nameof(Checkout));
            }

            var variantDisplay = !string.IsNullOrEmpty(resolvedVariant) ? $" ({resolvedVariant})" : "";
            TempData["SuccessMessage"] = $"Đã thêm \"{product.Name}{variantDisplay}\" vào giỏ hàng.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public async Task<IActionResult> UpdateQuantity(int productId, int quantity, string? selectedVariant = null)
        {
            var cart = GetCartItems();
            var item = cart.FirstOrDefault(x =>
                x.ProductId == productId &&
                string.Equals(x.SelectedVariant ?? "", selectedVariant ?? "", StringComparison.OrdinalIgnoreCase));

            if (item != null)
            {
                if (quantity <= 0)
                {
                    cart.Remove(item);
                }
                else
                {
                    var product = await _context.Products.FindAsync(productId);
                    var maxStock = product?.StockQuantity ?? 0;
                    var otherVariantsQty = cart.Where(x => x != item && x.ProductId == productId).Sum(x => x.Quantity);
                    var maxAllowed = Math.Max(0, maxStock - otherVariantsQty);

                    if (maxAllowed <= 0)
                    {
                        cart.Remove(item);
                        TempData["ErrorMessage"] = $"Sản phẩm \"{item.ProductName}\" vừa hết hàng khả dụng trong kho.";
                    }
                    else if (quantity > maxAllowed)
                    {
                        item.Quantity = maxAllowed;
                        TempData["ErrorMessage"] = $"Kho chỉ còn tối đa {maxStock} sản phẩm \"{item.ProductName}\".";
                    }
                    else
                    {
                        item.Quantity = quantity;
                    }
                }
                SaveCartItems(cart);
            }
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public IActionResult RemoveItem(int productId, string? selectedVariant = null)
        {
            var cart = GetCartItems();
            var item = cart.FirstOrDefault(x =>
                x.ProductId == productId &&
                string.Equals(x.SelectedVariant ?? "", selectedVariant ?? "", StringComparison.OrdinalIgnoreCase));

            if (item != null)
            {
                cart.Remove(item);
                SaveCartItems(cart);
                TempData["SuccessMessage"] = "Đã xóa sản phẩm khỏi giỏ hàng.";
            }
            return RedirectToAction(nameof(Index));
        }

        private IActionResult RedirectToCartOrReturnUrl(string? returnUrl)
        {
            if (!string.IsNullOrWhiteSpace(returnUrl) && Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public async Task<IActionResult> ApplyCoupon(string? couponCode, string? returnUrl = null)
        {
            if (string.IsNullOrWhiteSpace(couponCode))
            {
                HttpContext.Session.Remove(CouponSessionKey);
                TempData["SuccessMessage"] = "Đã gỡ mã giảm giá khỏi đơn hàng.";
                return RedirectToCartOrReturnUrl(returnUrl);
            }

            var code = couponCode.Trim().ToUpper();
            var coupon = await _context.Coupons.AsNoTracking().FirstOrDefaultAsync(c => c.Code == code && c.IsActive);

            if (coupon == null)
            {
                TempData["ErrorMessage"] = $"Mã ưu đãi \"{code}\" không tồn tại hoặc đang tạm khóa.";
                return RedirectToCartOrReturnUrl(returnUrl);
            }

            var today = DateTime.Now.Date;
            if (today < coupon.StartDate.Date)
            {
                TempData["ErrorMessage"] = $"Mã ưu đãi \"{code}\" chưa đến thời gian mở áp dụng (Bắt đầu từ {coupon.StartDate:dd/MM/yyyy}).";
                return RedirectToCartOrReturnUrl(returnUrl);
            }

            if (today > coupon.EndDate.Date)
            {
                TempData["ErrorMessage"] = $"Mã ưu đãi \"{code}\" đã hết hạn áp dụng từ ngày {coupon.EndDate:dd/MM/yyyy}.";
                return RedirectToCartOrReturnUrl(returnUrl);
            }

            if (coupon.UsedCount >= coupon.UsageLimit)
            {
                TempData["ErrorMessage"] = $"Mã ưu đãi \"{code}\" đã hết lượt sử dụng ({coupon.UsageLimit} lượt).";
                return RedirectToCartOrReturnUrl(returnUrl);
            }

            var items = GetCartItems();
            var subTotal = items.Sum(x => x.Total);
            if (subTotal < coupon.MinOrderAmount)
            {
                var needed = coupon.MinOrderAmount - subTotal;
                TempData["ErrorMessage"] = $"Mã \"{code}\" chỉ áp dụng cho đơn từ {coupon.MinOrderAmount:N0} đ. Vui lòng mua thêm {needed:N0} đ để dùng mã này.";
                return RedirectToCartOrReturnUrl(returnUrl);
            }

            HttpContext.Session.SetString(CouponSessionKey, code);
            var discountAmt = CalculateDiscount(subTotal, code, out _);
            TempData["SuccessMessage"] = $"Áp dụng mã \"{code}\" thành công: Giảm ngay {discountAmt:N0} đ ({coupon.Description}).";
            return RedirectToCartOrReturnUrl(returnUrl);
        }

        [HttpPost]
        public IActionResult RemoveCoupon(string? returnUrl = null)
        {
            HttpContext.Session.Remove(CouponSessionKey);
            TempData["SuccessMessage"] = "Đã gỡ mã giảm giá khỏi đơn hàng.";
            return RedirectToCartOrReturnUrl(returnUrl);
        }

        [HttpGet]
        public async Task<IActionResult> Checkout()
        {
            var cartVm = BuildCartViewModel();
            if (!cartVm.Items.Any())
            {
                TempData["ErrorMessage"] = "Giỏ hàng của bạn đang trống. Vui lòng chọn sản phẩm trước khi thanh toán.";
                return RedirectToAction("Index", "Product");
            }

            var userId = HttpContext.Session.GetInt32("USER_ID");
            if (!userId.HasValue)
            {
                TempData["ErrorMessage"] = "Vui lòng đăng nhập tài khoản khách hàng để tiến hành thanh toán đơn hàng.";
                return RedirectToAction("Login", "Account", new { returnUrl = "/Cart/Checkout" });
            }

            var defaultPayment = TempData["DefaultPaymentMethod"]?.ToString() ?? "COD";

            var vm = new CheckoutViewModel
            {
                Cart = cartVm,
                CouponCode = cartVm.CouponCode,
                PaymentMethod = defaultPayment
            };

            var user = await _context.Users.FindAsync(userId.Value);
            if (user != null)
            {
                vm.CustomerName = user.FullName;
                vm.CustomerPhone = user.Phone;
                vm.CustomerEmail = user.Email;
                vm.CompanyEmail = user.Email;
                vm.ShippingAddress = user.Address;
            }

            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Checkout(CheckoutViewModel model)
        {
            var userId = HttpContext.Session.GetInt32("USER_ID");
            if (!userId.HasValue)
            {
                TempData["ErrorMessage"] = "Phiên đăng nhập đã hết hạn. Vui lòng đăng nhập tài khoản để hoàn tất thanh toán.";
                return RedirectToAction("Login", "Account", new { returnUrl = "/Cart/Checkout" });
            }

            var cartVm = BuildCartViewModel();
            if (!cartVm.Items.Any())
            {
                return RedirectToAction(nameof(Index));
            }

            model.Cart = cartVm;
            ModelState.Remove(nameof(model.ShippingAddress));

            if (model.RequireVatInvoice)
            {
                if (string.IsNullOrWhiteSpace(model.CompanyName))
                {
                    ModelState.AddModelError(nameof(model.CompanyName), "Vui lòng nhập Tên công ty / Đơn vị xuất hóa đơn GTGT.");
                }
                if (string.IsNullOrWhiteSpace(model.CompanyTaxCode))
                {
                    ModelState.AddModelError(nameof(model.CompanyTaxCode), "Vui lòng nhập Mã số thuế doanh nghiệp.");
                }
                if (string.IsNullOrWhiteSpace(model.CompanyAddress))
                {
                    ModelState.AddModelError(nameof(model.CompanyAddress), "Vui lòng nhập Địa chỉ trụ sở đăng ký kinh doanh.");
                }
            }

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var fullShippingAddress = $"{model.AddressDetail.Trim()}, {model.WardName.Trim()}, {model.ProvinceName.Trim()}";
            model.ShippingAddress = fullShippingAddress;

            // Kiểm tra chặt chẽ tồn kho thực tế trước khi tạo đơn hàng (Gộp theo ProductId nếu có nhiều biến thể)
            var groupedItems = cartVm.Items.GroupBy(i => i.ProductId);
            foreach (var grp in groupedItems)
            {
                var totalQtyRequested = grp.Sum(x => x.Quantity);
                var stockProduct = await _context.Products.FindAsync(grp.Key);
                if (stockProduct == null || !stockProduct.IsActive || stockProduct.StockQuantity < totalQtyRequested)
                {
                    var sampleName = grp.First().ProductName;
                    TempData["ErrorMessage"] = $"Sản phẩm \"{sampleName}\" hiện chỉ còn {stockProduct?.StockQuantity ?? 0} sản phẩm trong kho, không đủ số lượng đặt mua ({totalQtyRequested}).";
                    return RedirectToAction(nameof(Index));
                }
            }

            var orderCode = "DMX" + DateTime.Now.ToString("yyyyMMddHHmmss");

            string? installmentInfo = null;
            if (model.PaymentMethod == "Installment")
            {
                var partnerLabel = model.InstallmentPartnerType == "FinanceCompany"
                    ? "Công ty tài chính (Duyệt CCCD)"
                    : "Thẻ tín dụng 0% (Visa/Mastercard/JCB)";
                var provider = string.IsNullOrWhiteSpace(model.InstallmentBankOrProvider) ? "Vietcombank" : model.InstallmentBankOrProvider.Trim();
                var months = model.InstallmentMonths > 0 ? model.InstallmentMonths : 6;
                var prepayPct = model.InstallmentPartnerType == "CreditCard" ? 0 : Math.Clamp(model.InstallmentPrepayPercent, 0, 70);
                var prepayAmt = Math.Round(cartVm.FinalTotal * prepayPct / 100m, 0);
                var remainAmt = Math.Max(0, cartVm.FinalTotal - prepayAmt);
                var monthlyAmt = Math.Round(remainAmt / months, 0);

                installmentInfo = $"{partnerLabel} - {provider} | Kỳ hạn: {months} tháng | Trả trước: {prepayPct}% ({prepayAmt:N0} đ) | Góp mỗi tháng: {monthlyAmt:N0} đ (Lãi suất 0%)";
            }

            var paymentStatus = model.PaymentMethod switch
            {
                "Banking" => "Chờ xác nhận chuyển khoản",
                "Installment" => "Chờ duyệt hồ sơ trả góp 0%",
                _ => "Thanh toán khi nhận hàng (COD)"
            };

            var order = new Order
            {
                OrderCode = orderCode,
                UserId = userId,
                CustomerName = model.CustomerName.Trim(),
                CustomerPhone = model.CustomerPhone.Trim(),
                CustomerEmail = model.CustomerEmail?.Trim(),
                ShippingAddress = fullShippingAddress,
                Note = model.Note?.Trim(),
                SubTotal = cartVm.SubTotal,
                DiscountAmount = cartVm.DiscountAmount,
                CouponCode = cartVm.CouponCode,
                VatAmount = cartVm.VatAmount,
                TotalAmount = cartVm.FinalTotal,
                PaymentMethod = model.PaymentMethod,
                InstallmentInfo = installmentInfo,
                RequireVatInvoice = model.RequireVatInvoice,
                CompanyName = model.RequireVatInvoice ? model.CompanyName?.Trim() : null,
                CompanyTaxCode = model.RequireVatInvoice ? model.CompanyTaxCode?.Trim() : null,
                CompanyAddress = model.RequireVatInvoice ? model.CompanyAddress?.Trim() : null,
                CompanyEmail = model.RequireVatInvoice ? (model.CompanyEmail?.Trim() ?? model.CustomerEmail?.Trim()) : null,
                PaymentStatus = paymentStatus,
                OrderStatus = "Chờ xác nhận",
                CreatedAt = DateTime.Now
            };

            foreach (var item in cartVm.Items)
            {
                order.OrderDetails.Add(new OrderDetail
                {
                    ProductId = item.ProductId,
                    ProductName = item.ProductName,
                    SelectedVariant = item.SelectedVariant,
                    ProductImage = item.ImageUrl,
                    UnitPrice = item.UnitPrice,
                    Quantity = item.Quantity
                });

                var product = await _context.Products.FindAsync(item.ProductId);
                if (product != null)
                {
                    product.StockQuantity = Math.Max(0, product.StockQuantity - item.Quantity);
                    product.SoldCount += item.Quantity;
                }
            }

            _context.Orders.Add(order);

            if (!string.IsNullOrWhiteSpace(order.CouponCode))
            {
                var appliedCoupon = await _context.Coupons.FirstOrDefaultAsync(c => c.Code == order.CouponCode);
                if (appliedCoupon != null)
                {
                    appliedCoupon.UsedCount += 1;
                }
            }

            await _context.SaveChangesAsync();

            HttpContext.Session.Remove(CartSessionKey);
            HttpContext.Session.Remove(CouponSessionKey);

            return RedirectToAction(nameof(OrderSuccess), new { code = order.OrderCode });
        }

        public async Task<IActionResult> OrderSuccess(string code)
        {
            if (string.IsNullOrWhiteSpace(code))
            {
                return RedirectToAction("Index", "Home");
            }

            var order = await _context.Orders
                .Include(o => o.OrderDetails)
                .FirstOrDefaultAsync(o => o.OrderCode == code);

            if (order == null)
            {
                return NotFound();
            }

            return View(order);
        }

        public async Task<IActionResult> TrackOrder(string? keyword)
        {
            var orders = new List<Order>();
            if (!string.IsNullOrWhiteSpace(keyword))
            {
                var kw = keyword.Trim();
                orders = await _context.Orders
                    .Include(o => o.OrderDetails)
                    .Where(o => o.OrderCode == kw || o.CustomerPhone == kw)
                    .OrderByDescending(o => o.CreatedAt)
                    .ToListAsync();
            }

            ViewBag.Keyword = keyword;
            return View(orders);
        }

        [HttpGet]
        public async Task<IActionResult> GetNewProvinces()
        {
            try
            {
                if (string.IsNullOrEmpty(_cachedProvincesJson))
                {
                    _cachedProvincesJson = await _httpClient.GetStringAsync("https://provinces.open-api.vn/api/v2/p/");
                }
                return Content(_cachedProvincesJson, "application/json; charset=utf-8");
            }
            catch
            {
                return StatusCode(502, new { error = "Không thể kết nối API đơn vị hành chính mới." });
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetNewWards(int code)
        {
            if (code <= 0)
            {
                return BadRequest();
            }

            try
            {
                if (!_cachedWardsJson.TryGetValue(code, out var json))
                {
                    json = await _httpClient.GetStringAsync($"https://provinces.open-api.vn/api/v2/p/{code}?depth=2");
                    _cachedWardsJson[code] = json;
                }
                return Content(json, "application/json; charset=utf-8");
            }
            catch
            {
                return StatusCode(502, new { error = "Không thể tải danh sách Phường/Xã mới." });
            }
        }
    }
}

