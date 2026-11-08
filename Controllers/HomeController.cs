using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebsiteDienMay.Data;
using WebsiteDienMay.Models;

namespace WebsiteDienMay.Controllers
{
    public class HomeController : Controller
    {
        private readonly ApplicationDbContext _context;

        public HomeController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var categories = await _context.Categories
                .Where(c => c.IsActive)
                .OrderBy(c => c.DisplayOrder)
                .ToListAsync();

            var brands = await _context.Brands
                .OrderBy(b => b.Id)
                .ToListAsync();

            var allProducts = await _context.Products
                .Include(p => p.Category)
                .Include(p => p.Brand)
                .Where(p => p.IsActive)
                .ToListAsync();

            var flashSaleProducts = allProducts
                .Where(p => p.IsFlashSale)
                .OrderByDescending(p => p.DiscountPercent)
                .Take(8)
                .ToList();

            var featuredProducts = allProducts
                .Where(p => p.IsFeatured)
                .OrderByDescending(p => p.SoldCount)
                .Take(10)
                .ToList();

            var categorySections = new Dictionary<Category, List<Product>>();
            foreach (var cat in categories)
            {
                var catProducts = allProducts
                    .Where(p => p.CategoryId == cat.Id)
                    .OrderByDescending(p => p.SoldCount)
                    .Take(4)
                    .ToList();

                if (catProducts.Any())
                {
                    categorySections.Add(cat, catProducts);
                }
            }

            var activeBanners = await _context.Banners
                .Where(b => b.IsActive)
                .OrderBy(b => b.DisplayOrder)
                .ThenByDescending(b => b.Id)
                .ToListAsync();

            var vm = new HomeViewModel
            {
                Categories = categories,
                Brands = brands,
                FlashSaleProducts = flashSaleProducts,
                FeaturedProducts = featuredProducts,
                CategorySections = categorySections,
                HeroBanners = activeBanners.Where(b => b.Position == "Hero").ToList(),
                SubBanners = activeBanners.Where(b => b.Position == "SubBanner").ToList()
            };

            return View(vm);
        }

        public IActionResult About()
        {
            return View();
        }

        public IActionResult WarrantyPolicy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
