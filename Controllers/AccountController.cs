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

            var input = model.Username.Trim().ToLower();
            var user = await _context.Users.FirstOrDefaultAsync(u =>
                (u.Username.ToLower() == input || u.Email.ToLower() == input || u.Phone == input) &&
                u.PasswordHash == model.Password &&
                u.IsActive);

            if (user == null)
            {
                ModelState.AddModelError(string.Empty, "Tên đăng nhập hoặc mật khẩu không chính xác.");
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

            var exists = await _context.Users.AnyAsync(u =>
                u.Username.ToLower() == model.Username.Trim().ToLower() ||
                u.Email.ToLower() == model.Email.Trim().ToLower() ||
                u.Phone == model.Phone.Trim());

            if (exists)
            {
                ModelState.AddModelError(string.Empty, "Tên đăng nhập, Email hoặc Số điện thoại đã được sử dụng.");
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
