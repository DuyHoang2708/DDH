using DDH.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DDH.Controllers
{
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
            if (ModelState.IsValid)
            {
                // Kiểm tra trùng tên đăng nhập hoặc email
                bool isExist = await _context.Accounts
                    .AnyAsync(a => a.Username == model.Username || a.Email == model.Email);

                if (isExist)
                {
                    ModelState.AddModelError("", "Tên đăng nhập hoặc Email đã tồn tại!");
                    return View(model);
                }

                _context.Accounts.Add(model);
                await _context.SaveChangesAsync();

                TempData["Success"] = "Đăng ký thành công! Vui lòng đăng nhập.";
                return RedirectToAction("Login");
            }

            return View(model);
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
                ViewBag.Error = "Vui lòng nhập đủ thông tin.";
                return View();
            }

            var user = await _context.Accounts
                .FirstOrDefaultAsync(a => a.Username == username && a.Password == password);

            if (user == null)
            {
                ViewBag.Error = "Tên đăng nhập hoặc mật khẩu không đúng!";
                return View();
            }

            // 🔒 Lưu Session
            HttpContext.Session.SetString("Username", user.Username);
            HttpContext.Session.SetString("FullName", user.FullName ?? "");
            HttpContext.Session.SetString("Email", user.Email ?? "");
            HttpContext.Session.SetInt32("Role", user.Role);

            TempData["Success"] = $"Xin chào, {user.FullName ?? user.Username}!";
            return RedirectToAction("Index", "Home");
        }
        public IActionResult Profile()
        {
            var username = HttpContext.Session.GetString("Username");
            if (string.IsNullOrEmpty(username))
            {
                return RedirectToAction("Login");
            }

            // Lấy thông tin tài khoản từ DB
            var user = _context.Accounts.FirstOrDefault(a => a.Username == username);
            if (user == null)
            {
                return RedirectToAction("Login");
            }

            return View(user);
        }

        // ==================== ĐĂNG XUẤT ====================
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            TempData["Success"] = "Đã đăng xuất!";
            return RedirectToAction("Index", "Home");
        }

        // ==================== KIỂM TRA QUYỀN (TUỲ CHỌN) ====================
        public IActionResult AccessDenied()
        {
            return View();
        }
    }
}
