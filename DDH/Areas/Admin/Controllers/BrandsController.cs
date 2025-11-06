using DDH.Filters;
using DDH.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DDH.Areas.Admin.Controllers
{
    [Area("Admin")]
    [AuthorizeRole( 1)]
    public class BrandsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public BrandsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // ===================== DANH SÁCH =====================
        public async Task<IActionResult> ListBrands()
        {
            var brands = await _context.Brands.ToListAsync();
            return View(brands);
        }

        // ===================== THÊM MỚI =====================
        [HttpGet]
        public IActionResult CreateBrands()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateBrands(Brand brand)
        {
            if (ModelState.IsValid)
            {
                _context.Brands.Add(brand);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Thêm thương hiệu thành công!";
                return RedirectToAction(nameof(ListBrands));
            }
            return View(brand);
        }

        // ===================== CẬP NHẬT =====================
        [HttpGet]
        public async Task<IActionResult> UpdateBrands(int id)
        {
            var brand = await _context.Brands.FindAsync(id);
            if (brand == null)
            {
                return NotFound();
            }
            return View(brand);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateBrands(int id, Brand brand)
        {
            if (id != brand.BrandId)
            {
                return BadRequest();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(brand);
                    await _context.SaveChangesAsync();
                    TempData["Success"] = "Cập nhật thương hiệu thành công!";
                    return RedirectToAction(nameof(ListBrands));
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!_context.Brands.Any(b => b.BrandId == id))
                    {
                        return NotFound();
                    }
                    throw;
                }
            }

            return View(brand);
        }

        // ===================== ẨN / HIỆN =====================
        [HttpPost]
        public async Task<IActionResult> ToggleStatus(int id)
        {
            var brand = await _context.Brands.FindAsync(id);
            if (brand == null)
            {
                return NotFound();
            }

            brand.IsActive = !brand.IsActive;
            _context.Brands.Update(brand);
            await _context.SaveChangesAsync();

            TempData["Success"] = brand.IsActive
                ? "Đã bật hiển thị thương hiệu."
                : "Đã ẩn thương hiệu.";

            return RedirectToAction(nameof(Index));
        }
    }
}
