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

        private decimal CalculateDiscount(decimal subTotal, string? couponCode)
        {
            if (string.IsNullOrWhiteSpace(couponCode) || subTotal <= 0) return 0;
            var code = couponCode.Trim().ToUpper();

            var coupon = _context.Coupons.FirstOrDefault(c => c.Code == code && c.IsActive);
            if (coupon == null) return 0;

            var now = DateTime.Now;
            if (now < coupon.StartDate || now > coupon.EndDate) return 0;
            if (coupon.UsedCount >= coupon.UsageLimit) return 0;
            if (subTotal < coupon.MinOrderAmount) return 0;

            if (coupon.DiscountType == "Percentage")
            {
                var discount = Math.Round(subTotal * (coupon.DiscountValue / 100m), 0);
                if (coupon.MaxDiscountAmount.HasValue && coupon.MaxDiscountAmount.Value > 0)
                {
                    discount = Math.Min(discount, coupon.MaxDiscountAmount.Value);
                }
                return discount;
            }
            else
            {
                return Math.Min(subTotal, coupon.DiscountValue);
            }
        }

        private CartViewModel BuildCartViewModel()
        {
            var items = GetCartItems();
            var coupon = HttpContext.Session.GetString(CouponSessionKey);
            var subTotal = items.Sum(x => x.Total);
            var discount = CalculateDiscount(subTotal, coupon);

            return new CartViewModel
            {
                Items = items,
                CouponCode = coupon,
                DiscountAmount = discount
            };
        }

        public IActionResult Index()
        {
            var vm = BuildCartViewModel();
            return View(vm);
        }

        [HttpPost]
        public async Task<IActionResult> AddToCart(int productId, int quantity = 1, bool buyNow = false)
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

            var reqQty = Math.Max(1, quantity);
            var cart = GetCartItems();
            var existing = cart.FirstOrDefault(x => x.ProductId == productId);
            if (existing != null)
            {
                var newQty = existing.Quantity + reqQty;
                if (newQty > product.StockQuantity)
                {
                    existing.Quantity = product.StockQuantity;
                    SaveCartItems(cart);
                    TempData["ErrorMessage"] = $"Kho hiện chỉ còn {product.StockQuantity} sản phẩm \"{product.Name}\".";
                    return RedirectToAction(nameof(Index));
                }
                existing.Quantity = newQty;
            }
            else
            {
                var allowedQty = Math.Min(reqQty, product.StockQuantity);
                cart.Add(new CartItemViewModel
                {
                    ProductId = product.Id,
                    ProductName = product.Name,
                    ProductCode = product.Code,
                    ImageUrl = product.ImageUrl,
                    UnitPrice = product.Price,
                    OriginalPrice = product.OriginalPrice,
                    Quantity = allowedQty
                });

                if (reqQty > product.StockQuantity)
                {
                    SaveCartItems(cart);
                    TempData["ErrorMessage"] = $"Kho hiện chỉ còn {product.StockQuantity} sản phẩm \"{product.Name}\".";
                    return RedirectToAction(nameof(Index));
                }
            }

            SaveCartItems(cart);

            if (buyNow)
            {
                if (!HttpContext.Session.GetInt32("USER_ID").HasValue)
                {
                    TempData["ErrorMessage"] = "Vui lòng đăng nhập tài khoản khách hàng để tiến hành thanh toán đơn hàng.";
                    return RedirectToAction("Login", "Account", new { returnUrl = "/Cart/Checkout" });
                }
                return RedirectToAction(nameof(Checkout));
            }

            TempData["SuccessMessage"] = $"Đã thêm \"{product.Name}\" vào giỏ hàng.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public async Task<IActionResult> UpdateQuantity(int productId, int quantity)
        {
            var cart = GetCartItems();
            var item = cart.FirstOrDefault(x => x.ProductId == productId);
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
                    if (maxStock <= 0)
                    {
                        cart.Remove(item);
                        TempData["ErrorMessage"] = $"Sản phẩm \"{item.ProductName}\" vừa hết hàng trong kho.";
                    }
                    else if (quantity > maxStock)
                    {
                        item.Quantity = maxStock;
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
        public IActionResult RemoveItem(int productId)
        {
            var cart = GetCartItems();
            var item = cart.FirstOrDefault(x => x.ProductId == productId);
            if (item != null)
            {
                cart.Remove(item);
                SaveCartItems(cart);
                TempData["SuccessMessage"] = "Đã xóa sản phẩm khỏi giỏ hàng.";
            }
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public async Task<IActionResult> ApplyCoupon(string? couponCode)
        {
            if (string.IsNullOrWhiteSpace(couponCode))
            {
                HttpContext.Session.Remove(CouponSessionKey);
                TempData["SuccessMessage"] = "Đã gỡ mã giảm giá.";
                return RedirectToAction(nameof(Index));
            }

            var code = couponCode.Trim().ToUpper();
            var coupon = await _context.Coupons.FirstOrDefaultAsync(c => c.Code == code && c.IsActive);

            if (coupon == null)
            {
                TempData["ErrorMessage"] = $"Mã ưu đãi \"{code}\" không tồn tại hoặc đã ngừng áp dụng.";
                return RedirectToAction(nameof(Index));
            }

            var now = DateTime.Now;
            if (now < coupon.StartDate || now > coupon.EndDate)
            {
                TempData["ErrorMessage"] = $"Mã ưu đãi \"{code}\" đã hết hạn áp dụng.";
                return RedirectToAction(nameof(Index));
            }

            if (coupon.UsedCount >= coupon.UsageLimit)
            {
                TempData["ErrorMessage"] = $"Mã ưu đãi \"{code}\" đã đạt giới hạn lượt sử dụng.";
                return RedirectToAction(nameof(Index));
            }

            var cartVm = BuildCartViewModel();
            if (cartVm.SubTotal < coupon.MinOrderAmount)
            {
                TempData["ErrorMessage"] = $"Mã \"{code}\" áp dụng cho đơn từ {coupon.MinOrderAmount:N0} đ. (Đơn hiện tại: {cartVm.SubTotal:N0} đ)";
                return RedirectToAction(nameof(Index));
            }

            HttpContext.Session.SetString(CouponSessionKey, code);
            TempData["SuccessMessage"] = $"Áp dụng mã \"{code}\" thành công ({coupon.Description}).";
            return RedirectToAction(nameof(Index));
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

            var vm = new CheckoutViewModel
            {
                Cart = cartVm,
                CouponCode = cartVm.CouponCode,
                PaymentMethod = "COD"
            };

            var user = await _context.Users.FindAsync(userId.Value);
            if (user != null)
            {
                vm.CustomerName = user.FullName;
                vm.CustomerPhone = user.Phone;
                vm.CustomerEmail = user.Email;
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

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var fullShippingAddress = $"{model.AddressDetail.Trim()}, {model.WardName.Trim()}, {model.ProvinceName.Trim()}";
            model.ShippingAddress = fullShippingAddress;

            // Kiểm tra chặt chẽ tồn kho thực tế trước khi tạo đơn hàng
            foreach (var item in cartVm.Items)
            {
                var stockProduct = await _context.Products.FindAsync(item.ProductId);
                if (stockProduct == null || !stockProduct.IsActive || stockProduct.StockQuantity < item.Quantity)
                {
                    TempData["ErrorMessage"] = $"Sản phẩm \"{item.ProductName}\" hiện chỉ còn {stockProduct?.StockQuantity ?? 0} sản phẩm trong kho, không đủ số lượng đặt mua ({item.Quantity}).";
                    return RedirectToAction(nameof(Index));
                }
            }

            var orderCode = "DMX" + DateTime.Now.ToString("yyyyMMddHHmmss");

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
                TotalAmount = cartVm.FinalTotal,
                PaymentMethod = model.PaymentMethod,
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

