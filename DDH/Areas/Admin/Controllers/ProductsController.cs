using DDH.Filters;
using DDH.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DDH.Areas.Admin.Controllers
{
    [Area("Admin")]
    [AuthorizeRole(1)]
    public class ProductsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _environment;

        public ProductsController(ApplicationDbContext context, IWebHostEnvironment environment)
        {
            _context = context;
            _environment = environment;
        }

        // ===================== DANH SÁCH SẢN PHẨM =====================
        public async Task<IActionResult> ListProducts()
        {
            var products = await _context.Products
                .Include(p => p.Category)
                .Include(p => p.Brand)
                .ToListAsync();

            return View(products);
        }

        // ===================== TẠO MỚI SẢN PHẨM =====================
        [HttpGet]
        public IActionResult CreateProducts()
        {
            ViewBag.Categories = _context.Categories
                .Where(c => c.IsActive)
                .ToList();

            ViewBag.Brands = _context.Brands
                .Where(b => b.IsActive)
                .ToList();

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateProducts(Product product, IFormFile? imageFile)
        {
            if (ModelState.IsValid)
            {
                // 🖼️ Xử lý upload ảnh
                if (imageFile != null && imageFile.Length > 0)
                {
                    string uploadFolder = Path.Combine(_environment.WebRootPath, "images", "products");
                    Directory.CreateDirectory(uploadFolder);
                    string uniqueFileName = Guid.NewGuid().ToString() + Path.GetExtension(imageFile.FileName);
                    string filePath = Path.Combine(uploadFolder, uniqueFileName);

                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await imageFile.CopyToAsync(stream);
                    }

                    product.ImageUrl = "/images/products/" + uniqueFileName;
                }

                product.IsActive = true;
                _context.Products.Add(product);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "✅ Thêm sản phẩm thành công!";
                return RedirectToAction(nameof(ListProducts));
            }

            ViewBag.Categories = _context.Categories.ToList();
            ViewBag.Brands = _context.Brands.ToList();
            return View(product);
        }

        // ===================== CẬP NHẬT SẢN PHẨM =====================
        [HttpGet]
        public async Task<IActionResult> UpdateProducts(int? id)
        {
            if (id == null)
                return NotFound();

            var product = await _context.Products.FindAsync(id);
            if (product == null)
                return NotFound();

            ViewBag.Categories = _context.Categories.ToList();
            ViewBag.Brands = _context.Brands.ToList();

            return View(product);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateProducts(int id, Product product, IFormFile? imageFile)
        {
            if (id != product.ProductId)
                return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    var existingProduct = await _context.Products.AsNoTracking().FirstOrDefaultAsync(p => p.ProductId == id);
                    if (existingProduct == null)
                        return NotFound();

                    // 🖼️ Nếu người dùng tải ảnh mới
                    if (imageFile != null && imageFile.Length > 0)
                    {
                        string uploadFolder = Path.Combine(_environment.WebRootPath, "images", "products");
                        Directory.CreateDirectory(uploadFolder);
                        string uniqueFileName = Guid.NewGuid().ToString() + Path.GetExtension(imageFile.FileName);
                        string filePath = Path.Combine(uploadFolder, uniqueFileName);

                        using (var stream = new FileStream(filePath, FileMode.Create))
                        {
                            await imageFile.CopyToAsync(stream);
                        }

                        product.ImageUrl = "/images/products/" + uniqueFileName;
                    }
                    else
                    {
                        // Giữ lại ảnh cũ nếu không tải ảnh mới
                        product.ImageUrl = existingProduct.ImageUrl;
                    }

                    _context.Update(product);
                    await _context.SaveChangesAsync();

                    TempData["SuccessMessage"] = "✅ Cập nhật sản phẩm thành công!";
                    return RedirectToAction(nameof(ListProducts));
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!_context.Products.Any(e => e.ProductId == product.ProductId))
                        return NotFound();
                    else
                        throw;
                }
            }

            ViewBag.Categories = _context.Categories.ToList();
            ViewBag.Brands = _context.Brands.ToList();
            return View(product);
        }
        // ===================== TÌM KIẾM SẢN PHẨM =====================
        


        // ===================== ẨN / HIỆN SẢN PHẨM =====================
        [HttpPost]
        public async Task<IActionResult> ToggleStatus(int id)
        {
            var product = await _context.Products.FindAsync(id);
            if (product == null)
                return Json(new { success = false, message = "Không tìm thấy sản phẩm!" });

            product.IsActive = !product.IsActive;
            _context.Update(product);
            await _context.SaveChangesAsync();

            return Json(new
            {
                success = true,
                isActive = product.IsActive,
                message = product.IsActive
                    ? "🔓 Sản phẩm đã được hiển thị!"
                    : "🔒 Sản phẩm đã được ẩn!"
            });
        }

    }
}
