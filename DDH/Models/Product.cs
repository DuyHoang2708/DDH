    using Microsoft.EntityFrameworkCore;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    namespace DDH.Models
    {
        public class Product
        {
            [Key]
            public int ProductId { get; set; }

            [Required(ErrorMessage = "Tên sản phẩm là bắt buộc")]
            public string Name { get; set; }

            public string? Description { get; set; }

            [Required(ErrorMessage = "Giá là bắt buộc")]
            [Range(0, double.MaxValue, ErrorMessage = "Giá phải lớn hơn hoặc bằng 0")]
            [Precision(18, 2)]
            public decimal Price { get; set; }
            // 🔥 Thêm dòng này để lưu ảnh sản phẩm
            public string? ImageUrl { get; set; }
            public bool IsActive { get; set; } = true; // true = hiển thị, false = ẩn

            [Required]
            public int CategoryId { get; set; }

            [ForeignKey("CategoryId")]
            public Category? Category { get; set; }

            [Required]
            public int BrandId { get; set; }

            [ForeignKey("BrandId")]
            public Brand? Brand { get; set; }
        public int ViewCount { get; set; } = 0;

        public int Stock { get; set; } = 0;
        }
    }
