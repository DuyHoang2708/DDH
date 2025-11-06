using DDH.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using DDH.Filters;
using System.Linq;

namespace DDH.Areas.Admin.Controllers
{
    [Area("Admin")]
    [AuthorizeRole(1)]
    public class HomeController : Controller
    {
        private readonly ApplicationDbContext _context;

        public HomeController(ApplicationDbContext context)
        {
            _context = context;
        }

        public IActionResult Dashboard()
        {
            try
            {
                // ==== 1️⃣ Thống kê tổng quan ====
                ViewBag.TotalUsers = _context.Accounts.Count();
                ViewBag.TotalProducts = _context.Products.Count();
                ViewBag.TotalOrders = _context.Orders.Count();

                // Nếu bạn có cột TotalAmount hoặc OrderDetails để tính tổng doanh thu:
                ViewBag.TotalRevenue = _context.Orders
                    .Where(o => o.Status == "Đã giao")
                    .Sum(o => (decimal?)o.TotalAmount) ?? 0;

                // ==== 2️⃣ Đơn hàng gần đây (top 5) ====
                ViewBag.RecentOrders = _context.Orders
                    .Include(o => o.Account)
                    .OrderByDescending(o => o.CreatedAt)
                    .Take(5)
                    .ToList();

                // ==== 3️⃣ Sản phẩm nổi bật (top 5 view cao nhất) ====
                ViewBag.TopViewedProducts = _context.Products
                    .OrderByDescending(p => p.ViewCount)
                    .Take(5)
                    .ToList();
            }
            catch (Exception ex)
            {
                ViewBag.Error = "Không thể tải dữ liệu thống kê: " + ex.Message;
            }

            return View();
        }
    }
}
