namespace DDH.Models
{
    public class CheckoutViewModel
    {
        public List<CartItem> CartItems { get; set; } = new List<CartItem>();
        public decimal TotalAmount { get; set; }

        // Dữ liệu người dùng
        public string FullName { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public string Address { get; set; }

        // Hình thức thanh toán
        public string PaymentMethod { get; set; }  // "COD", "VNPAY", "MOMO"
    }
}
