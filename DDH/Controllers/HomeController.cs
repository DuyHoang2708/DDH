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

        public async Task<IActionResult> Index()
        {
            // Lấy tối đa 8 sản phẩm đang hoạt động
            var products = await _context.Products
                .Where(p => p.IsActive)
                .Include(p => p.Category)
                .Include(p => p.Brand)
                .Take(8)
                .ToListAsync();

            return View(products);
        }
    }
}
