using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebsiteDienMay.Data;
using WebsiteDienMay.Models;

namespace WebsiteDienMay.Controllers
{
    public class ProductController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ProductController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index(
            int? categoryId,
            int? brandId,
            string? keyword,
            string? priceRange,
            string? sort,
            int page = 1)
        {
            var query = _context.Products
                .Include(p => p.Category)
                .Include(p => p.Brand)
                .Where(p => p.IsActive)
                .AsQueryable();

            if (categoryId.HasValue && categoryId.Value > 0)
            {
                query = query.Where(p => p.CategoryId == categoryId.Value);
            }

            if (brandId.HasValue && brandId.Value > 0)
            {
                query = query.Where(p => p.BrandId == brandId.Value);
            }

            if (!string.IsNullOrWhiteSpace(keyword))
            {
                var kw = keyword.Trim().ToLower();
                query = query.Where(p =>
                    p.Name.ToLower().Contains(kw) ||
                    p.Code.ToLower().Contains(kw) ||
                    p.ShortDescription.ToLower().Contains(kw) ||
                    (p.Brand != null && p.Brand.Name.ToLower().Contains(kw)) ||
                    (p.Category != null && p.Category.Name.ToLower().Contains(kw)));
            }

            if (!string.IsNullOrWhiteSpace(priceRange))
            {
                switch (priceRange)
                {
                    case "under-5m":
                        query = query.Where(p => p.Price < 5000000);
                        break;
                    case "5m-10m":
                        query = query.Where(p => p.Price >= 5000000 && p.Price < 10000000);
                        break;
                    case "10m-15m":
                        query = query.Where(p => p.Price >= 10000000 && p.Price < 15000000);
                        break;
                    case "15m-25m":
                        query = query.Where(p => p.Price >= 15000000 && p.Price <= 25000000);
                        break;
                    case "over-25m":
                        query = query.Where(p => p.Price > 25000000);
                        break;
                }
            }

            query = sort switch
            {
                "price-asc" => query.OrderBy(p => p.Price),
                "price-desc" => query.OrderByDescending(p => p.Price),
                "bestseller" => query.OrderByDescending(p => p.SoldCount),
                "discount" => query.OrderByDescending(p => (p.OriginalPrice - p.Price)),
                _ => query.OrderByDescending(p => p.IsFeatured).ThenByDescending(p => p.SoldCount)
            };

            const int pageSize = 12;
            var totalItems = await query.CountAsync();
            var totalPages = Math.Max(1, (int)Math.Ceiling(totalItems / (double)pageSize));
            page = Math.Clamp(page, 1, totalPages);

            var products = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var categories = await _context.Categories
                .Where(c => c.IsActive)
                .OrderBy(c => c.DisplayOrder)
                .ToListAsync();

            var brands = await _context.Brands
                .OrderBy(b => b.Name)
                .ToListAsync();

            Category? currentCategory = null;
            if (categoryId.HasValue)
            {
                currentCategory = categories.FirstOrDefault(c => c.Id == categoryId.Value);
            }

            var vm = new ProductListViewModel
            {
                Products = products,
                Categories = categories,
                Brands = brands,
                SelectedCategoryId = categoryId,
                CurrentCategory = currentCategory,
                SelectedBrandId = brandId,
                SearchKeyword = keyword,
                PriceRange = priceRange,
                SortOrder = sort,
                CurrentPage = page,
                TotalPages = totalPages,
                TotalItems = totalItems
            };

            return View(vm);
        }

        public async Task<IActionResult> Detail(int id)
        {
            var product = await _context.Products
                .Include(p => p.Category)
                .Include(p => p.Brand)
                .FirstOrDefaultAsync(p => p.Id == id && p.IsActive);

            if (product == null)
            {
                return NotFound();
            }

            var relatedProducts = await _context.Products
                .Include(p => p.Brand)
                .Where(p => p.CategoryId == product.CategoryId && p.Id != product.Id && p.IsActive)
                .OrderByDescending(p => p.SoldCount)
                .Take(4)
                .ToListAsync();

            var reviews = await _context.ProductReviews
                .Where(r => r.ProductId == product.Id && r.IsApproved)
                .OrderByDescending(r => r.CreatedAt)
                .ToListAsync();

            var specList = new List<KeyValuePair<string, string>>();
            if (!string.IsNullOrWhiteSpace(product.Specifications))
            {
                var lines = product.Specifications.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
                foreach (var line in lines)
                {
                    var parts = line.Split(':', 2);
                    if (parts.Length == 2)
                    {
                        specList.Add(new KeyValuePair<string, string>(parts[0].Trim(), parts[1].Trim()));
                    }
                    else
                    {
                        specList.Add(new KeyValuePair<string, string>("Thông số", line.Trim()));
                    }
                }
            }

            var promotions = new List<string>();
            if (!string.IsNullOrWhiteSpace(product.PromotionText))
            {
                promotions = product.PromotionText
                    .Split(';', StringSplitOptions.RemoveEmptyEntries)
                    .Select(s => s.Trim())
                    .ToList();
            }

            var gallery = new List<string> { product.ImageUrl };
            if (!string.IsNullOrWhiteSpace(product.ExtraImages))
            {
                var extras = product.ExtraImages
                    .Split(';', StringSplitOptions.RemoveEmptyEntries)
                    .Select(s => s.Trim())
                    .Where(s => !string.IsNullOrEmpty(s));
                gallery.AddRange(extras);
            }

            var vm = new ProductDetailViewModel
            {
                Product = product,
                RelatedProducts = relatedProducts,
                Reviews = reviews,
                SpecList = specList,
                Promotions = promotions,
                GalleryImages = gallery.Distinct().ToList()
            };

            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddReview(int productId, string customerName, string? customerPhone, int rating, string comment)
        {
            var product = await _context.Products.FindAsync(productId);
            if (product == null)
            {
                return NotFound();
            }

            if (string.IsNullOrWhiteSpace(customerName) || string.IsNullOrWhiteSpace(comment))
            {
                TempData["ErrorMessage"] = "Vui lòng nhập đầy đủ họ tên và nội dung đánh giá.";
                return RedirectToAction(nameof(Detail), new { id = productId });
            }

            rating = Math.Clamp(rating, 1, 5);
            var userId = HttpContext.Session.GetInt32("USER_ID");

            var review = new ProductReview
            {
                ProductId = productId,
                UserId = userId,
                CustomerName = customerName.Trim(),
                CustomerPhone = string.IsNullOrWhiteSpace(customerPhone) ? "Khách mua hàng" : customerPhone.Trim(),
                Rating = rating,
                Comment = comment.Trim(),
                CreatedAt = DateTime.Now,
                IsApproved = true
            };

            _context.ProductReviews.Add(review);

            product.RatingCount += 1;
            product.RatingAverage = Math.Round(((product.RatingAverage * (product.RatingCount - 1)) + rating) / product.RatingCount, 1);

            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Cảm ơn bạn đã gửi đánh giá cho sản phẩm này.";
            return RedirectToAction(nameof(Detail), new { id = productId });
        }

        // ================= TÌM KIẾM TRỰC TIẾP (LIVE SEARCH / AUTOCOMPLETE) =================
        [HttpGet]
        public async Task<IActionResult> LiveSearch(string? keyword)
        {
            if (string.IsNullOrWhiteSpace(keyword) || keyword.Trim().Length < 2)
            {
                return Json(new { success = true, products = new List<object>(), totalMatches = 0 });
            }

            var kw = keyword.Trim().ToLower();
            var matches = await _context.Products
                .Include(p => p.Category)
                .Include(p => p.Brand)
                .Where(p => p.IsActive && (
                    p.Name.ToLower().Contains(kw) ||
                    p.Code.ToLower().Contains(kw) ||
                    (p.Category != null && p.Category.Name.ToLower().Contains(kw)) ||
                    (p.Brand != null && p.Brand.Name.ToLower().Contains(kw))
                ))
                .OrderByDescending(p => p.SoldCount)
                .Take(6)
                .Select(p => new LiveSearchResultItem
                {
                    Id = p.Id,
                    Name = p.Name,
                    Code = p.Code,
                    Slug = p.Slug,
                    ImageUrl = p.ImageUrl,
                    Price = p.Price,
                    FormattedPrice = p.Price.ToString("N0") + " đ",
                    OriginalPrice = p.OriginalPrice,
                    FormattedOriginalPrice = p.OriginalPrice.ToString("N0") + " đ",
                    DiscountPercent = p.OriginalPrice > p.Price ? (int)Math.Round((1 - (p.Price / p.OriginalPrice)) * 100) : 0,
                    CategoryName = p.Category != null ? p.Category.Name : "",
                    BrandName = p.Brand != null ? p.Brand.Name : "",
                    StockQuantity = p.StockQuantity,
                    IsInStock = p.StockQuantity > 0
                })
                .ToListAsync();

            var totalMatches = await _context.Products
                .Where(p => p.IsActive && (
                    p.Name.ToLower().Contains(kw) ||
                    p.Code.ToLower().Contains(kw)
                ))
                .CountAsync();

            return Json(new { success = true, products = matches, totalMatches });
        }

        // ================= SO SÁNH SẢN PHẨM (PRODUCT COMPARISON) =================
        private const string CompareSessionKey = "COMPARE_PRODUCT_IDS";

        private List<int> GetCompareIdsFromSession()
        {
            var raw = HttpContext.Session.GetString(CompareSessionKey);
            if (string.IsNullOrWhiteSpace(raw))
            {
                return new List<int>();
            }

            return raw.Split(',', StringSplitOptions.RemoveEmptyEntries)
                .Select(s => int.TryParse(s, out var id) ? id : 0)
                .Where(id => id > 0)
                .Distinct()
                .Take(3)
                .ToList();
        }

        private void SaveCompareIdsToSession(List<int> ids)
        {
            var cleaned = ids.Distinct().Take(3).ToList();
            if (cleaned.Any())
            {
                HttpContext.Session.SetString(CompareSessionKey, string.Join(",", cleaned));
            }
            else
            {
                HttpContext.Session.Remove(CompareSessionKey);
            }
        }

        [HttpGet]
        public async Task<IActionResult> Compare(string? ids)
        {
            var idList = new List<int>();
            if (!string.IsNullOrWhiteSpace(ids))
            {
                idList = ids.Split(new[] { ',', '-' }, StringSplitOptions.RemoveEmptyEntries)
                    .Select(s => int.TryParse(s.Trim(), out var id) ? id : 0)
                    .Where(id => id > 0)
                    .Distinct()
                    .Take(3)
                    .ToList();

                if (idList.Any())
                {
                    SaveCompareIdsToSession(idList);
                }
            }
            else
            {
                idList = GetCompareIdsFromSession();
            }

            var products = new List<Product>();
            if (idList.Any())
            {
                products = await _context.Products
                    .Include(p => p.Category)
                    .Include(p => p.Brand)
                    .Where(p => idList.Contains(p.Id) && p.IsActive)
                    .ToListAsync();

                products = products.OrderBy(p => idList.IndexOf(p.Id)).ToList();
            }

            var specKeys = new List<string>();
            var productSpecs = new Dictionary<int, Dictionary<string, string>>();

            foreach (var prod in products)
            {
                var dict = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
                if (!string.IsNullOrWhiteSpace(prod.Specifications))
                {
                    var lines = prod.Specifications.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
                    foreach (var line in lines)
                    {
                        var parts = line.Split(':', 2);
                        if (parts.Length == 2)
                        {
                            var key = parts[0].Trim();
                            var val = parts[1].Trim();
                            if (!string.IsNullOrWhiteSpace(key))
                            {
                                dict[key] = val;
                                if (!specKeys.Contains(key, StringComparer.OrdinalIgnoreCase))
                                {
                                    specKeys.Add(key);
                                }
                            }
                        }
                    }
                }
                productSpecs[prod.Id] = dict;
            }

            Category? cat = products.FirstOrDefault()?.Category;
            var candidateProducts = new List<Product>();
            if (cat != null)
            {
                var currentIds = products.Select(p => p.Id).ToList();
                candidateProducts = await _context.Products
                    .Where(p => p.CategoryId == cat.Id && !currentIds.Contains(p.Id) && p.IsActive)
                    .OrderByDescending(p => p.SoldCount)
                    .Take(12)
                    .ToListAsync();
            }
            else
            {
                var currentIds = products.Select(p => p.Id).ToList();
                candidateProducts = await _context.Products
                    .Where(p => !currentIds.Contains(p.Id) && p.IsActive)
                    .OrderByDescending(p => p.SoldCount)
                    .Take(12)
                    .ToListAsync();
            }

            var vm = new ProductCompareViewModel
            {
                Products = products,
                SpecKeys = specKeys,
                ProductSpecs = productSpecs,
                CandidateProducts = candidateProducts,
                Category = cat
            };

            return View(vm);
        }

        [HttpPost]
        public async Task<IActionResult> AddToCompare(int productId)
        {
            var product = await _context.Products
                .Include(p => p.Category)
                .FirstOrDefaultAsync(p => p.Id == productId && p.IsActive);

            if (product == null)
            {
                return Json(new { success = false, message = "Không tìm thấy sản phẩm." });
            }

            var ids = GetCompareIdsFromSession();
            if (ids.Contains(productId))
            {
                return Json(new { success = true, message = "Sản phẩm đã có trong danh sách so sánh.", count = ids.Count });
            }

            if (ids.Count >= 3)
            {
                return Json(new { success = false, message = "Hệ thống hỗ trợ so sánh tối đa 3 sản phẩm cùng lúc. Vui lòng bỏ bớt 1 sản phẩm.", count = ids.Count });
            }

            ids.Add(productId);
            SaveCompareIdsToSession(ids);

            return Json(new { success = true, message = $"Đã thêm \"{product.Name}\" vào so sánh.", count = ids.Count, productId = product.Id });
        }

        [HttpPost]
        public IActionResult RemoveFromCompare(int productId)
        {
            var ids = GetCompareIdsFromSession();
            ids.Remove(productId);
            SaveCompareIdsToSession(ids);

            return Json(new { success = true, count = ids.Count, message = "Đã bỏ sản phẩm khỏi so sánh." });
        }

        [HttpPost]
        public IActionResult ClearCompare()
        {
            HttpContext.Session.Remove(CompareSessionKey);
            return Json(new { success = true, count = 0, message = "Đã làm trống danh sách so sánh." });
        }

        [HttpGet]
        public async Task<IActionResult> GetCompareBar()
        {
            var ids = GetCompareIdsFromSession();
            if (!ids.Any())
            {
                return Json(new { count = 0, items = new List<object>() });
            }

            var products = await _context.Products
                .Where(p => ids.Contains(p.Id))
                .Select(p => new
                {
                    id = p.Id,
                    name = p.Name,
                    imageUrl = p.ImageUrl,
                    price = p.Price.ToString("N0") + " đ"
                })
                .ToListAsync();

            return Json(new { count = products.Count, items = products });
        }
    }
}

