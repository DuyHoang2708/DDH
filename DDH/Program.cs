using DDH.Models;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// ===================== KẾT NỐI DATABASE =====================
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// ===================== CẤU HÌNH SESSION =====================
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30); // Hết hạn sau 30 phút
    options.Cookie.HttpOnly = true;                 // Chỉ truy cập bằng HTTP, không JS
    options.Cookie.IsEssential = true;              // Bắt buộc (vì có thể liên quan đến đăng nhập)
});

// ===================== THÊM MVC SERVICE =====================
builder.Services.AddControllersWithViews();

var app = builder.Build();

// ===================== XỬ LÝ LỖI =====================
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
}

// ===================== PIPELINE CỦA ỨNG DỤNG =====================
app.UseStaticFiles();  // Cho phép đọc file trong wwwroot
app.UseRouting();      // Kích hoạt định tuyến

// ⚡ DÙNG SESSION TRƯỚC AUTHORIZATION
app.UseSession();
app.UseAuthorization();

// ===================== CẤU HÌNH ROUTE CHO AREAS =====================
// 🔹 Định tuyến cho khu vực (Admin)
app.MapControllerRoute(
    name: "areas",
    pattern: "{area:exists}/{controller=Home}/{action=Dashboard}/{id?}");

// 🔹 Định tuyến mặc định cho trang chính (người dùng thường)
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

// ===================== CHẠY ỨNG DỤNG =====================
app.Run();
