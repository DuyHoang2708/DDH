using DDH.Services;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Text;

namespace DDH.Controllers
{
    [Route("payment")]
    public class PaymentController : Controller
    {
        private readonly IConfiguration _config;

        public PaymentController(IConfiguration config)
        {
            _config = config;
        }

        // ✅ VNPAY RETURN
        [HttpGet("vnpay-return")]
        public IActionResult VnpayReturn()
        {
            var allParams = Request.Query.ToDictionary(k => k.Key, v => v.Value.ToString());
            string code = Request.Query["vnp_ResponseCode"];
            string message = code == "00" ? "✅ Thanh toán VNPAY thành công" : "❌ Thanh toán VNPAY thất bại";

            var debugInfo = new StringBuilder();
            debugInfo.AppendLine(message);
            debugInfo.AppendLine("Chi tiết phản hồi:");
            foreach (var p in allParams)
                debugInfo.AppendLine($"{p.Key}: {p.Value}");

            return Content(debugInfo.ToString(), "text/plain", Encoding.UTF8);
        }


        // ✅ MOMO RETURN
        [HttpGet("momo-return")]
        public IActionResult MomoReturn()
        {
            string code = Request.Query["resultCode"];
            if (code == "0")
                return RedirectToAction("PaymentSuccess");

            return Content("❌ Thanh toán MoMo thất bại");
        }

        public IActionResult PaymentSuccess()
        {
            return View();
        }

        // (các hàm CreateVnpayPayment và CreateMomoPayment giữ nguyên)
    }
}
