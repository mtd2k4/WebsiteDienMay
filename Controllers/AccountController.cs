using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebsiteDienMay.Data;
using WebsiteDienMay.Models;

namespace WebsiteDienMay.Controllers
{
    public class AccountController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AccountController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public IActionResult Login(string? returnUrl = null)
        {
            var userId = HttpContext.Session.GetInt32("USER_ID");
            var role = HttpContext.Session.GetString("USER_ROLE");

            if (userId.HasValue)
            {
                if (!string.IsNullOrEmpty(returnUrl) && returnUrl.StartsWith("/Admin", StringComparison.OrdinalIgnoreCase) && role != "Admin")
                {
                    return RedirectToAction(nameof(AccessDenied));
                }
                return role == "Admin" ? RedirectToAction("Index", "Admin") : RedirectToAction("Index", "Home");
            }
            return View(new LoginViewModel { ReturnUrl = returnUrl });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var input = model.Username.Trim();
            var user = await _context.Users.FirstOrDefaultAsync(u =>
                (u.Username.ToLower() == input.ToLower() || u.Email.ToLower() == input.ToLower() || u.Phone == input));

            if (user == null)
            {
                ModelState.AddModelError(nameof(model.Username), "Tài khoản hoặc số điện thoại này chưa được đăng ký trong hệ thống.");
                return View(model);
            }

            if (!user.IsActive)
            {
                ModelState.AddModelError(string.Empty, "Tài khoản của bạn hiện đang bị tạm khóa. Vui lòng liên hệ tổng đài 1800 1061 để được hỗ trợ.");
                return View(model);
            }

            if (user.PasswordHash != model.Password)
            {
                ModelState.AddModelError(nameof(model.Password), "Mật khẩu không chính xác. Vui lòng kiểm tra lại.");
                return View(model);
            }

            // Nếu đang cố đăng nhập vào trang Admin bằng tài khoản Khách hàng -> Từ chối ngay
            if (!string.IsNullOrEmpty(model.ReturnUrl) &&
                model.ReturnUrl.StartsWith("/Admin", StringComparison.OrdinalIgnoreCase) &&
                !string.Equals(user.Role, "Admin", StringComparison.OrdinalIgnoreCase))
            {
                TempData["ErrorMessage"] = "Tài khoản khách hàng không được phép truy cập vào trang Quản trị hệ thống.";
                return View(model);
            }

            HttpContext.Session.SetInt32("USER_ID", user.Id);
            HttpContext.Session.SetString("USER_NAME", user.Username);
            HttpContext.Session.SetString("USER_FULLNAME", user.FullName);
            HttpContext.Session.SetString("USER_ROLE", user.Role);

            TempData["SuccessMessage"] = $"Xin chào {user.FullName}, bạn đã đăng nhập thành công.";

            if (!string.IsNullOrEmpty(model.ReturnUrl) && Url.IsLocalUrl(model.ReturnUrl))
            {
                return Redirect(model.ReturnUrl);
            }

            if (user.Role == "Admin")
            {
                return RedirectToAction("Index", "Admin");
            }

            return RedirectToAction("Index", "Home");
        }

        [HttpGet]
        public IActionResult AccessDenied()
        {
            return View();
        }

        [HttpGet]
        public IActionResult Register(string? returnUrl = null)
        {
            if (HttpContext.Session.GetInt32("USER_ID").HasValue)
            {
                return RedirectToAction("Index", "Home");
            }
            ViewBag.ReturnUrl = returnUrl;
            return View(new RegisterViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model, string? returnUrl = null)
        {
            ViewBag.ReturnUrl = returnUrl;
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var usernameExists = await _context.Users.AnyAsync(u => u.Username.ToLower() == model.Username.Trim().ToLower());
            if (usernameExists)
            {
                ModelState.AddModelError(nameof(model.Username), "Tên đăng nhập này đã tồn tại trong hệ thống. Vui lòng chọn tên khác.");
            }

            var phoneExists = await _context.Users.AnyAsync(u => u.Phone == model.Phone.Trim());
            if (phoneExists)
            {
                ModelState.AddModelError(nameof(model.Phone), "Số điện thoại này đã được đăng ký cho tài khoản khác.");
            }

            var emailExists = await _context.Users.AnyAsync(u => u.Email.ToLower() == model.Email.Trim().ToLower());
            if (emailExists)
            {
                ModelState.AddModelError(nameof(model.Email), "Địa chỉ Email này đã được sử dụng cho một tài khoản khác.");
            }

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var user = new User
            {
                FullName = model.FullName.Trim(),
                Username = model.Username.Trim(),
                Email = model.Email.Trim(),
                Phone = model.Phone.Trim(),
                Address = model.Address.Trim(),
                PasswordHash = model.Password,
                Role = "Customer",
                CreatedAt = DateTime.Now,
                IsActive = true
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            HttpContext.Session.SetInt32("USER_ID", user.Id);
            HttpContext.Session.SetString("USER_NAME", user.Username);
            HttpContext.Session.SetString("USER_FULLNAME", user.FullName);
            HttpContext.Session.SetString("USER_ROLE", user.Role);

            TempData["SuccessMessage"] = "Đăng ký tài khoản thành công.";
            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }
            return RedirectToAction("Index", "Home");
        }

        public async Task<IActionResult> Profile()
        {
            var userId = HttpContext.Session.GetInt32("USER_ID");
            if (!userId.HasValue)
            {
                return RedirectToAction(nameof(Login), new { returnUrl = "/Account/Profile" });
            }

            var user = await _context.Users.FindAsync(userId.Value);
            if (user == null)
            {
                return RedirectToAction(nameof(Logout));
            }

            var orders = await _context.Orders
                .Include(o => o.OrderDetails)
                .Where(o => o.UserId == user.Id || o.CustomerPhone == user.Phone)
                .OrderByDescending(o => o.CreatedAt)
                .ToListAsync();

            ViewBag.Orders = orders;
            return View(user);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateProfile(string fullName, string email, string phone, string address, string? newPassword)
        {
            var userId = HttpContext.Session.GetInt32("USER_ID");
            if (!userId.HasValue)
            {
                return RedirectToAction(nameof(Login));
            }

            var user = await _context.Users.FindAsync(userId.Value);
            if (user == null)
            {
                return NotFound();
            }

            user.FullName = fullName.Trim();
            user.Email = email.Trim();
            user.Phone = phone.Trim();
            user.Address = address.Trim();

            if (!string.IsNullOrWhiteSpace(newPassword) && newPassword.Length >= 6)
            {
                user.PasswordHash = newPassword;
            }

            await _context.SaveChangesAsync();
            HttpContext.Session.SetString("USER_FULLNAME", user.FullName);

            TempData["SuccessMessage"] = "Cập nhật thông tin tài khoản thành công.";
            return RedirectToAction(nameof(Profile));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CancelOrder(int orderId)
        {
            var userId = HttpContext.Session.GetInt32("USER_ID");
            if (!userId.HasValue)
            {
                return RedirectToAction(nameof(Login));
            }

            var order = await _context.Orders
                .Include(o => o.OrderDetails)
                .FirstOrDefaultAsync(o => o.Id == orderId);

            if (order != null && order.OrderStatus == "Chờ xác nhận")
            {
                order.OrderStatus = "Đã hủy";
                foreach (var detail in order.OrderDetails)
                {
                    var product = await _context.Products.FindAsync(detail.ProductId);
                    if (product != null)
                    {
                        product.StockQuantity += detail.Quantity;
                        product.SoldCount = Math.Max(0, product.SoldCount - detail.Quantity);
                    }
                }
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = $"Đã hủy đơn hàng {order.OrderCode}.";
            }

            return RedirectToAction(nameof(Profile));
        }

        public IActionResult Logout()
        {
            HttpContext.Session.Remove("USER_ID");
            HttpContext.Session.Remove("USER_NAME");
            HttpContext.Session.Remove("USER_FULLNAME");
            HttpContext.Session.Remove("USER_ROLE");
            TempData["SuccessMessage"] = "Bạn đã đăng xuất khỏi hệ thống.";
            return RedirectToAction("Index", "Home");
        }
    }
}
