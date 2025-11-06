using DDH.Filters;
using DDH.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;

namespace DDH.Controllers
{
    [AuthorizeRole(0, 1)]
    public class FavoritesController : Controller
    {
        private readonly ApplicationDbContext _context;

        public FavoritesController(ApplicationDbContext context)
        {
            _context = context;
        }

        // ✅ Thêm vào yêu thích (AJAX)
        [HttpPost]
        public IActionResult Add(int productId)
        {
            var accountId = HttpContext.Session.GetInt32("AccountId");
            if (accountId == null)
            {
                return Json(new { success = false, message = "Bạn cần đăng nhập để thêm sản phẩm yêu thích." });
            }

            var product = _context.Products.Find(productId);
            if (product == null)
            {
                return Json(new { success = false, message = "Sản phẩm không tồn tại." });
            }

            var existing = _context.Favorites
                .FirstOrDefault(f => f.AccountId == accountId && f.ProductId == productId);

            if (existing != null)
            {
                // Nếu đã có thì xóa (toggle bỏ yêu thích)
                _context.Favorites.Remove(existing);
                _context.SaveChanges();
                return Json(new { success = true, removed = true, message = "Đã bỏ yêu thích." });
            }

            // Nếu chưa có thì thêm mới
            var favorite = new Favorite
            {
                AccountId = accountId.Value,
                ProductId = productId,
                CreatedAt = DateTime.Now
            };

            _context.Favorites.Add(favorite);
            _context.SaveChanges();

            return Json(new { success = true, removed = false, message = "Đã thêm vào yêu thích!" });
        }

        // ✅ Danh sách yêu thích (View trang hiển thị)
        public IActionResult ListFavorites()
        {
            var accountId = HttpContext.Session.GetInt32("AccountId");
            if (accountId == null)
            {
                TempData["Error"] = "Vui lòng đăng nhập để xem danh sách yêu thích.";
                return RedirectToAction("Login", "Account");
            }

            var favorites = _context.Favorites
                .Where(f => f.AccountId == accountId)
                .Include(f => f.Product)
                .ToList();

            return View(favorites); // Trả về view ListFavorites.cshtml
        }

        // ✅ Xóa khỏi yêu thích (dùng form POST)
        [HttpPost]
        public IActionResult RemoveFromFavorites(int productId)
        {
            var accountId = HttpContext.Session.GetInt32("AccountId");
            if (accountId == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var favorite = _context.Favorites
                .FirstOrDefault(f => f.AccountId == accountId && f.ProductId == productId);

            if (favorite != null)
            {
                _context.Favorites.Remove(favorite);
                _context.SaveChanges();
            }

            return RedirectToAction("ListFavorites");
        }
    }
}
