using DDH.Models;
using DDH.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

namespace DDH.Controllers
{
    public class CheckoutController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IConfiguration _config;

        public CheckoutController(IConfiguration config, ApplicationDbContext context)
        {
            _config = config;
            _context = context; // ✅ bây giờ _context đã tồn tại
        }


        // ✅ Trang hiển thị thông tin thanh toán
        public IActionResult Index()
        {
            var username = HttpContext.Session.GetString("Username");

            // Lấy giỏ hàng từ session
            var sessionData = HttpContext.Session.GetString("CART");
            var cart = new List<CartItem>();
            if (!string.IsNullOrEmpty(sessionData))
                cart = JsonConvert.DeserializeObject<List<CartItem>>(sessionData);

            if (cart.Count == 0)
            {
                TempData["Error"] = "Giỏ hàng trống!";
                return RedirectToAction("ListCart", "Cart");
            }

            // Tính tổng tiền
            decimal totalAmount = cart.Sum(c => c.Total);

            // Lấy thông tin user
            var account = _context.Accounts.FirstOrDefault(a => a.Username == username);

            // Truyền dữ liệu sang view
            ViewBag.Amount = totalAmount;
            ViewBag.OrderId = "DH" + DateTime.Now.Ticks;
            ViewBag.FullName = account?.FullName ?? "";
            ViewBag.Phone = account?.Phone ?? "";
            ViewBag.Email = account?.Email ?? "";
            ViewBag.Address = ""; // trống để user nhập

            // Truyền danh sách giỏ hàng sang view để hiển thị sản phẩm
            return View(cart);
        }


        // ✅ Tạo URL VNPAY
        [HttpPost]
        public IActionResult CreatePaymentUrl(decimal amount, string orderId, string fullName, string phone, string address)
        {
            // Kiểm tra bắt buộc nhập địa chỉ
            if (string.IsNullOrWhiteSpace(address))
            {
                TempData["Error"] = "⚠️ Bạn phải nhập địa chỉ giao hàng!";
                return RedirectToAction("Index");
            }

            // Kiểm tra tổng tiền hợp lệ (VNPAY yêu cầu >= 5,000 VND)
            if (amount < 5000)
            {
                TempData["Error"] = "Tổng tiền phải tối thiểu 5,000 VND";
                return RedirectToAction("Index");
            }

            var vnp = new VnPayLibrary();
            var vnpUrl = _config["PaymentGateways:Vnpay:Url"];
            var returnUrl = _config["PaymentGateways:Vnpay:ReturnUrl"];
            var tmnCode = _config["PaymentGateways:Vnpay:TmnCode"];
            var hashSecret = _config["PaymentGateways:Vnpay:HashSecret"];

            var txnRef = orderId ?? DateTime.Now.Ticks.ToString();
            var createDate = DateTime.Now.ToString("yyyyMMddHHmmss");

            vnp.AddRequestData("vnp_Version", "2.1.0");
            vnp.AddRequestData("vnp_Command", "pay");
            vnp.AddRequestData("vnp_TmnCode", tmnCode);
            vnp.AddRequestData("vnp_Amount", ((long)(amount * 100)).ToString());
            vnp.AddRequestData("vnp_CreateDate", createDate);
            vnp.AddRequestData("vnp_CurrCode", "VND");
            vnp.AddRequestData("vnp_IpAddr", HttpContext.Connection.RemoteIpAddress?.ToString());
            vnp.AddRequestData("vnp_Locale", "vn");

            // Thêm địa chỉ vào OrderInfo
            vnp.AddRequestData("vnp_OrderInfo", $"Thanh toán đơn hàng #{txnRef} - {fullName} - {phone} - {address}");

            vnp.AddRequestData("vnp_OrderType", "other");
            vnp.AddRequestData("vnp_ReturnUrl", returnUrl);
            vnp.AddRequestData("vnp_TxnRef", txnRef);

            string paymentUrl = vnp.CreateRequestUrl(vnpUrl, hashSecret);
            return Redirect(paymentUrl);
        }

        // ✅ Xử lý khi VNPAY redirect về
        public IActionResult ReturnVnpay()
        {
            var vnp = new VnPayLibrary();
            var hashSecret = _config["PaymentGateways:Vnpay:HashSecret"];

            foreach (var (key, value) in Request.Query)
            {
                if (!string.IsNullOrEmpty(key) && key.StartsWith("vnp_"))
                    vnp.AddResponseData(key, value);
            }

            string vnp_SecureHash = Request.Query["vnp_SecureHash"];
            bool isValidSignature = vnp.ValidateSignature(vnp_SecureHash, hashSecret);

            if (isValidSignature)
            {
                string responseCode = vnp.GetResponseData("vnp_ResponseCode");
                if (responseCode == "00")
                {
                    ViewBag.Message = "✅ Thanh toán thành công!";
                }
                else
                {
                    ViewBag.Message = "❌ Thanh toán thất bại. Mã lỗi: " + responseCode;
                }
            }
            else
            {
                ViewBag.Message = "⚠️ Sai chữ ký bảo mật!";
            }

            return View();
        }
    }
}
