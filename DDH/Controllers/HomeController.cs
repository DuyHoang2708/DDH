using DDH.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DDH.Controllers
{
    public class HomeController : Controller
    {
        private readonly ApplicationDbContext _context;

        public HomeController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> Index(string? search, int? categoryId, int? brandId, decimal? minPrice, decimal? maxPrice)
        {
            // Đổ dữ liệu cho bộ lọc
            ViewBag.Categories = await _context.Categories.ToListAsync();
            ViewBag.Brands = await _context.Brands.ToListAsync();
            ViewBag.Keyword = search; // 👈 Giữ lại từ khóa tìm kiếm để hiển thị lại trong ô input

            // Truy vấn ban đầu
            var query = _context.Products
                .Include(p => p.Category)
                .Include(p => p.Brand)
                .Where(p => p.IsActive)
                .AsQueryable();

            // 🔍 Tìm theo tên
            if (!string.IsNullOrEmpty(search))
            {
                query = query.Where(p => p.Name.Contains(search) ||
                                         (p.Description != null && p.Description.Contains(search)));
            }

            // 🧩 Lọc danh mục
            if (categoryId.HasValue)
                query = query.Where(p => p.CategoryId == categoryId.Value);

            // 🏷️ Lọc thương hiệu
            if (brandId.HasValue)
                query = query.Where(p => p.BrandId == brandId.Value);

            // 💰 Lọc giá
            if (minPrice.HasValue)
                query = query.Where(p => p.Price >= minPrice.Value);
            if (maxPrice.HasValue)
                query = query.Where(p => p.Price <= maxPrice.Value);

            // ✅ Nếu có từ khóa hoặc filter thì hiển thị tất cả, nếu không thì chỉ lấy 8 sp mới
            bool hasFilter = !string.IsNullOrEmpty(search) || categoryId.HasValue || brandId.HasValue || minPrice.HasValue || maxPrice.HasValue;

            var products = await query
                .OrderByDescending(p => p.ProductId)
                .Take(hasFilter ? int.MaxValue : 8)
                .ToListAsync();

            return View(products);
        }
    }
}
