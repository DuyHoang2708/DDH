using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using DDH.Models;
using DDH.Filters;
using System.Linq;
using System.Threading.Tasks;
using System;

namespace DDH.Areas.Admin.Controllers
{
    [Area("Admin")]
    [AuthorizeRole(1)] // ✅ Chỉ cho Admin truy cập
    public class UsersController : Controller
    {
        private readonly ApplicationDbContext _context;

        public UsersController(ApplicationDbContext context)
        {
            _context = context;
        }

        // ==================== DANH SÁCH NGƯỜI DÙNG ====================
        public async Task<IActionResult> ListUser(string? search, string? status, string? role)
        {
            var users = await _context.Accounts.ToListAsync();

            // 🧮 Tính thống kê
            ViewBag.TotalUsers = users.Count;
            ViewBag.ActiveUsers = users.Count(u => u.IsActive);
            ViewBag.LockedUsers = users.Count(u => !u.IsActive);
            ViewBag.AdminCount = users.Count(u => u.Role == 1);

            // 🔍 Lọc dữ liệu
            if (!string.IsNullOrEmpty(search))
            {
                users = users.Where(u =>
                    (u.FullName != null && u.FullName.Contains(search)) ||
                    (u.Email != null && u.Email.Contains(search)) ||
                    (u.Phone != null && u.Phone.Contains(search))
                ).ToList();
            }

            if (!string.IsNullOrEmpty(status))
            {
                bool isActive = bool.Parse(status);
                users = users.Where(u => u.IsActive == isActive).ToList();
            }

            if (!string.IsNullOrEmpty(role))
            {
                if (role == "Admin")
                    users = users.Where(u => u.Role == 1).ToList();
                else if (role == "User")
                    users = users.Where(u => u.Role == 0).ToList();
            }

            // Trả về view
            return View(users);
        }


        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var user = await _context.Accounts.FindAsync(id);
            if (user == null) return NotFound();
            return View(user);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Account updatedUser)
        {
            if (!ModelState.IsValid)
                return View(updatedUser);

            var user = await _context.Accounts.FindAsync(updatedUser.AccountId);
            if (user == null) return NotFound();

            user.FullName = updatedUser.FullName;
            user.Email = updatedUser.Email;
            user.Phone = updatedUser.Phone;
            user.Role = updatedUser.Role;
            user.IsActive = updatedUser.IsActive;

            await _context.SaveChangesAsync();

            TempData["Success"] = "Cập nhật người dùng thành công!";
            return RedirectToAction(nameof(ListUser));
        }

        // ==================== XÓA NGƯỜI DÙNG ====================
        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            var user = await _context.Accounts.FindAsync(id);
            if (user == null) return NotFound();

            _context.Accounts.Remove(user);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Xóa người dùng thành công!";
            return RedirectToAction(nameof(ListUser));
        }

        [HttpGet]
        public async Task<IActionResult> ToggleActive(int id)
        {
            var account = await _context.Accounts.FindAsync(id);
            if (account == null)
            {
                return NotFound();
            }

            account.IsActive = !account.IsActive; // 🔁 Đổi trạng thái
            await _context.SaveChangesAsync();

            TempData["Success"] = account.IsActive ? "Đã mở khóa tài khoản!" : "Đã khóa tài khoản!";
            return RedirectToAction("ListUser");
        }


        // ==================== THỐNG KÊ NGƯỜI DÙNG ====================
        public IActionResult Statistics()
        {
            var total = _context.Accounts.Count();
            var active = _context.Accounts.Count(u => u.IsActive);
            var inactive = total - active;
            var admins = _context.Accounts.Count(u => u.Role == 1);
            var users = _context.Accounts.Count(u => u.Role == 0);

            ViewBag.Total = total;
            ViewBag.Active = active;
            ViewBag.Inactive = inactive;
            ViewBag.Admins = admins;
            ViewBag.Users = users;

            return View();
        }
    }
}
