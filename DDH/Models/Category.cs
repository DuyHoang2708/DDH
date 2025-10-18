using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace DDH.Models
{
    public class Category
    {
        [Key]
        public int CategoryId { get; set; }

        [Required(ErrorMessage = "Tên danh mục là bắt buộc")]
        [StringLength(100, ErrorMessage = "Tên danh mục không được vượt quá 100 ký tự")]
        public string Name { get; set; }

        

        public bool IsActive { get; set; } = true; // true = hiển thị, false = ẩn

        // 🔁 Quan hệ 1-n với Product
        public ICollection<Product>? Products { get; set; }
    }
}
