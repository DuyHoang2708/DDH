using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.WebUtilities;
using System.Net;

namespace DDH.Services
{
    public class VnPayLibrary
    {
        private readonly SortedList<string, string> _requestData = new SortedList<string, string>();
        private readonly SortedList<string, string> _responseData = new SortedList<string, string>();

        public void AddRequestData(string key, string value)
        {
            if (!string.IsNullOrEmpty(value))
                _requestData.Add(key, value);
        }

        public string CreateRequestUrl(string baseUrl, string hashSecret)
        {
            var query = new StringBuilder();
            foreach (var kv in _requestData)
            {
                query.Append($"{kv.Key}={System.Net.WebUtility.UrlEncode(kv.Value)}&");

            }

            var signData = query.ToString().TrimEnd('&');
            var sign = HmacSHA512(hashSecret, signData);
            return $"{baseUrl}?{signData}&vnp_SecureHash={sign}";
        }

        public void AddResponseData(string key, string value)
        {
            if (!string.IsNullOrEmpty(value))
                _responseData.Add(key, value);
        }

        public string GetResponseData(string key)
        {
            return _responseData.ContainsKey(key) ? _responseData[key] : string.Empty;
        }

        public bool ValidateSignature(string receivedHash, string secretKey)
        {
            var data = string.Join('&', _responseData
                .Where(x => !x.Key.Equals("vnp_SecureHash", StringComparison.InvariantCultureIgnoreCase))
                .Select(x => $"{x.Key}={x.Value}"));

            var myHash = HmacSHA512(secretKey, data);
            return myHash.Equals(receivedHash, StringComparison.InvariantCultureIgnoreCase);
        }

        private static string HmacSHA512(string key, string data)
        {
            using var hmac = new HMACSHA512(Encoding.UTF8.GetBytes(key));
            byte[] hashValue = hmac.ComputeHash(Encoding.UTF8.GetBytes(data));
            StringBuilder sb = new StringBuilder();
            foreach (var b in hashValue)
                sb.Append(b.ToString("x2"));
            return sb.ToString();
        }
    }
}
