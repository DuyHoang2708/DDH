using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.EntityFrameworkCore;
using DDH.Models;

namespace DDH.Filters
{
    public class AuthorizeRoleAttribute : Attribute, IAuthorizationFilter
    {
        private readonly int[] _roles;

        public AuthorizeRoleAttribute(params int[] roles)
        {
            _roles = roles;
        }

        public void OnAuthorization(AuthorizationFilterContext context)
        {
            var httpContext = context.HttpContext;
            var routeData = context.RouteData;

            string controller = routeData.Values["controller"]?.ToString() ?? "";
            string action = routeData.Values["action"]?.ToString() ?? "";
            string? area = routeData.Values["area"]?.ToString();

            // ⚠️ Cho phép truy cập các trang công khai
            if (controller == "Account" &&
                (action == "Login" || action == "Register" || action == "AccessDenied"))
            {
                return;
            }

            // 🔐 Lấy thông tin từ session
            var accountId = httpContext.Session.GetInt32("AccountId");
            var role = httpContext.Session.GetInt32("Role");

            // ⛔ Chưa đăng nhập
            if (accountId == null || role == null)
            {
                context.Result = new RedirectToActionResult("Login", "Account", new { area = "" });
                return;
            }

            // ⚙️ Kiểm tra tài khoản còn hoạt động không
            var db = httpContext.RequestServices.GetService(typeof(ApplicationDbContext)) as ApplicationDbContext;
            if (db != null)
            {
                var user = db.Accounts.AsNoTracking().FirstOrDefault(a => a.AccountId == accountId);
                if (user == null || !user.IsActive)
                {
                    // 🚫 Xóa session nếu bị khóa
                    httpContext.Session.Clear();
                    context.Result = new RedirectToActionResult("Login", "Account", new { area = "" });
                    return;
                }
            }

            // ⛔ Nếu không đúng quyền
            if (_roles.Length > 0 && !_roles.Contains(role.Value))
            {
                context.Result = new RedirectToActionResult("AccessDenied", "Account", new { area = "" });
                return;
            }
        }
    }
}
