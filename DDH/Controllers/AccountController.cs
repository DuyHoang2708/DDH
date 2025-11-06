using DDH.Filters;
using DDH.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

namespace DDH.Controllers
{
    [AuthorizeRole(0, 1)]
    public class AccountController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AccountController(ApplicationDbContext context)
        {
            _context = context;
        }

        // ==================== ĐĂNG KÝ ====================
        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(Account model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // 🔎 Kiểm tra trùng tên đăng nhập hoặc email
            bool isExist = await _context.Accounts
                .AnyAsync(a => a.Username == model.Username || a.Email == model.Email);

            if (isExist)
            {
                ModelState.AddModelError("", "Tên đăng nhập hoặc Email đã tồn tại!");
                return View(model);
            }

            // 🔐 Mã hóa mật khẩu bằng hàm trong model
            model.SetHashedPassword();

            // Lưu vào database
            _context.Accounts.Add(model);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Đăng ký thành công! Vui lòng đăng nhập.";
            return RedirectToAction("Login");
        }

        // ==================== ĐĂNG NHẬP ====================
        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(string username, string password)
        {
            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
            {
                ViewBag.Error = "Vui lòng nhập đầy đủ thông tin.";
                return View();
            }

            string hashedPassword = Account.HashPassword(password);

            var user = await _context.Accounts
                .FirstOrDefaultAsync(a => a.Username == username && a.Password == hashedPassword);

            if (user == null)
            {
                ViewBag.Error = "Tên đăng nhập hoặc mật khẩu không đúng!";
                return View();
            }

            // ✅ Kiểm tra tài khoản có bị khóa hay không
            if (!user.IsActive)
            {
                ViewBag.Error = "Tài khoản của bạn đã bị khóa. Vui lòng liên hệ quản trị viên!";
                return View();
            }

            // 🔒 Lưu session thông tin người dùng
            HttpContext.Session.SetInt32("AccountId", user.AccountId);
            HttpContext.Session.SetString("Username", user.Username);
            HttpContext.Session.SetString("FullName", user.FullName ?? "");
            HttpContext.Session.SetString("Email", user.Email ?? "");
            HttpContext.Session.SetInt32("Role", user.Role);

            // 🧭 Chuyển hướng theo Role
            if (user.Role == 1)
            {
                TempData["Success"] = $"Chào mừng quản trị viên {user.FullName ?? user.Username}!";
                return RedirectToAction("ListProducts", "Products", new { area = "Admin" });
            }
            else
            {
                TempData["Success"] = $"Xin chào, {user.FullName ?? user.Username}!";
                return RedirectToAction("Index", "Home");
            }
        }



        // ==================== TRANG CÁ NHÂN ====================
        [HttpGet]
        public IActionResult Profile()
        {
            var username = HttpContext.Session.GetString("Username");
            if (string.IsNullOrEmpty(username))
            {
                return RedirectToAction("Login");
            }

            var user = _context.Accounts.FirstOrDefault(a => a.Username == username);
            if (user == null)
            {
                return RedirectToAction("Login");
            }

            return View(user);
        }

        // ==================== CẬP NHẬT THÔNG TIN CÁ NHÂN ====================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Profile(Account updatedAccount)
        {
            var username = HttpContext.Session.GetString("Username");
            if (string.IsNullOrEmpty(username))
            {
                return RedirectToAction("Login");
            }

            var account = await _context.Accounts.FirstOrDefaultAsync(a => a.Username == username);
            if (account == null)
            {
                return RedirectToAction("Login");
            }

            // Cập nhật các thông tin cơ bản

            account.FullName = updatedAccount.FullName;
            account.Email = updatedAccount.Email;
            account.Phone = updatedAccount.Phone;

            await _context.SaveChangesAsync();

            TempData["Success"] = "Cập nhật thông tin cá nhân thành công!";
            return RedirectToAction("Profile");
        }

        // ==================== ĐỔI MẬT KHẨU ====================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangePassword(string oldPassword, string newPassword)
        {
            var username = HttpContext.Session.GetString("Username");
            if (string.IsNullOrEmpty(username))
            {
                return RedirectToAction("Login");
            }

            var account = await _context.Accounts.FirstOrDefaultAsync(a => a.Username == username);
            if (account == null)
            {
                return RedirectToAction("Login");
            }

            // Kiểm tra mật khẩu cũ
            string oldHashed = Account.HashPassword(oldPassword);
            if (account.Password != oldHashed)
            {
                TempData["Error"] = "Mật khẩu cũ không đúng!";
                return RedirectToAction("Profile");
            }

            // Cập nhật mật khẩu mới
            account.Password = newPassword;
            account.SetHashedPassword();
            await _context.SaveChangesAsync();

            TempData["Success"] = "Đổi mật khẩu thành công!";
            return RedirectToAction("Profile");
        }

        // ==================== ĐĂNG XUẤT ====================
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            TempData["Success"] = "Đã đăng xuất!";
            return RedirectToAction("Index", "Home");
        }

        // ==================== TRANG CẤM TRUY CẬP ====================
        public IActionResult AccessDenied()
        {
            return View();
        }
    }
}
