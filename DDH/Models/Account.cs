using System.ComponentModel.DataAnnotations;

namespace DDH.Models
{
public class Account
{
    [Key]
    public int AccountId { get; set; }

    [Required(ErrorMessage = "Tên đăng nhập là bắt buộc")]
    public string Username { get; set; }

    [Required(ErrorMessage = "Mật khẩu là bắt buộc")]
    [DataType(DataType.Password)]
    public string Password { get; set; }

    [Required]
    [EmailAddress]
    public string Email { get; set; }

    public string FullName { get; set; }
        public string? Phone { get; set; }

        // 0 = user, 1 = admin
        public int Role { get; set; } = 0;
}
}
