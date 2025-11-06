using DDH.Filters;
using DDH.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DDH.Areas.Admin.Controllers
{
    [Area("Admin")]
    [AuthorizeRole(1)]
    public class CategoryController : Controller
    {
        private readonly ApplicationDbContext _context;

        public CategoryController(ApplicationDbContext context)
        {
            _context = context;
        }

        // ===================== DANH SÁCH =====================
        public async Task<IActionResult> ListCategory()
        {
            var categories = await _context.Categories
                .Include(c => c.Products) // nếu muốn hiển thị số lượng sản phẩm
                .ToListAsync();
            return View(categories);
        }

        // ===================== TẠO MỚI =====================
        [HttpGet]
        public IActionResult CreateCategory()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateCategory(Category category)
        {
            if (ModelState.IsValid)
            {
                _context.Categories.Add(category);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Thêm danh mục thành công!";
                return RedirectToAction(nameof(ListCategory));
            }

            TempData["ErrorMessage"] = "Vui lòng kiểm tra lại thông tin nhập.";
            return View(category);
        }

        // ===================== CẬP NHẬT =====================
        [HttpGet]
        public async Task<IActionResult> UpdateCategory(int id)
        {
            var category = await _context.Categories.FindAsync(id);
            if (category == null)
                return NotFound();

            return View(category);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateCategory(Category category)
        {
            if (ModelState.IsValid)
            {
                var existingCategory = await _context.Categories.FindAsync(category.CategoryId);
                if (existingCategory == null)
                {
                    TempData["ErrorMessage"] = "Không tìm thấy danh mục!";
                    return RedirectToAction(nameof(Index));
                }

                existingCategory.Name = category.Name;
                existingCategory.IsActive = category.IsActive;

                _context.Categories.Update(existingCategory);
                await _context.SaveChangesAsync();

                TempData["Success"] = "Cập nhật danh mục thành công!";
                return RedirectToAction(nameof(ListCategory));
            }

            TempData["ErrorMessage"] = "Vui lòng kiểm tra lại thông tin nhập.";
            return View(category);
        }

        // ===================== ẨN/HIỆN =====================
        [HttpPost]
        public async Task<IActionResult> ToggleStatus(int id)
        {
            var category = await _context.Categories.FindAsync(id);
            if (category == null)
                return Json(new { success = false });

            category.IsActive = !category.IsActive;
            await _context.SaveChangesAsync();

            return Json(new { success = true, isActive = category.IsActive });
        }
    }
}
