using DDH.Filters;
using DDH.Models;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace DDH.Controllers
{
    [AuthorizeRole(0, 1)]
    public class CartController : Controller
    {
        private readonly ApplicationDbContext _context;
        private const string CART_KEY = "CART";

        public CartController(ApplicationDbContext context)
        {
            _context = context;
        }

        private List<CartItem> GetCart()
        {
            var sessionData = HttpContext.Session.GetString(CART_KEY);
            if (sessionData != null)
                return JsonConvert.DeserializeObject<List<CartItem>>(sessionData) ?? new List<CartItem>();

            return new List<CartItem>();
        }

        private void SaveCart(List<CartItem> cart)
        {
            var jsonData = JsonConvert.SerializeObject(cart);
            HttpContext.Session.SetString(CART_KEY, jsonData);
        }

        // 🛒 Hiển thị danh sách giỏ hàng
        public IActionResult ListCart()
        {
            var cart = GetCart();
            ViewBag.Total = cart.Sum(c => c.Total);
            return View(cart);
        }

        // ➕ Thêm sản phẩm vào giỏ hàng
        [HttpPost]
        public IActionResult AddToCart(int productId, int quantity = 1)
        {
            var product = _context.Products.FirstOrDefault(p => p.ProductId == productId && p.IsActive);
            if (product == null)
                return NotFound();

            var cart = GetCart();
            var item = cart.FirstOrDefault(c => c.ProductId == productId);

            if (item != null)
            {
                item.Quantity += quantity;
            }
            else
            {
                cart.Add(new CartItem
                {
                    ProductId = product.ProductId,
                    ProductName = product.Name,
                    Image = product.ImageUrl, // ✅ Đúng thuộc tính
                    Price = product.Price,
                    Quantity = quantity
                });

            }

            SaveCart(cart);
            TempData["Message"] = "Đã thêm sản phẩm vào giỏ hàng!";
            return RedirectToAction("ListCart", "Cart");
        }


        // ❌ Xóa 1 sản phẩm
        public IActionResult Remove(int productId)
        {
            var cart = GetCart();
            var item = cart.FirstOrDefault(c => c.ProductId == productId);
            if (item != null)
                cart.Remove(item);

            SaveCart(cart);
            return RedirectToAction("ListCart");
        }

        // 🧹 Xóa toàn bộ giỏ hàng
        public IActionResult Clear()
        {
            SaveCart(new List<CartItem>());
            return RedirectToAction("ListCart");
        }
    }
}
