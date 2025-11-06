using System.Security.Cryptography;
using System.Text;

namespace DDH.Services
{
    public static class PaymentService
    {
        /// <summary>
        /// Tạo mã HMAC SHA512 (dùng cho VNPAY)
        /// </summary>
        public static string HmacSHA512(string key, string data)
        {
            if (string.IsNullOrEmpty(key) || string.IsNullOrEmpty(data))
                return string.Empty;

            byte[] keyBytes = Encoding.UTF8.GetBytes(key);
            byte[] dataBytes = Encoding.UTF8.GetBytes(data);

            using (var hmac = new HMACSHA512(keyBytes))
            {
                byte[] hashValue = hmac.ComputeHash(dataBytes);
                return BitConverter.ToString(hashValue).Replace("-", "").ToLower();
            }
        }

        /// <summary>
        /// Tạo mã HMAC SHA256 (dành cho cổng khác, ví dụ MOMO)
        /// </summary>
        public static string HmacSHA256(string key, string data)
        {
            if (string.IsNullOrEmpty(key) || string.IsNullOrEmpty(data))
                return string.Empty;

            byte[] keyBytes = Encoding.UTF8.GetBytes(key);
            byte[] dataBytes = Encoding.UTF8.GetBytes(data);

            using (var hmac = new HMACSHA256(keyBytes))
            {
                byte[] hashValue = hmac.ComputeHash(dataBytes);
                return BitConverter.ToString(hashValue).Replace("-", "").ToLower();
            }
        }
    }
}
