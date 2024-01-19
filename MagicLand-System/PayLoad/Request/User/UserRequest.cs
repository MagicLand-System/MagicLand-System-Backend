using System.ComponentModel.DataAnnotations;

namespace MagicLand_System.PayLoad.Request.User
{

    public class UserRequest
    {
        [RegularExpression("^[A-Z][a-z]*( [A-Z][a-z]*)*$", ErrorMessage = "FullName Cần Phải Viết Hoa Đầu Mỗi Ký Chữ")]
        public string? FullName { get; set; }
        [DataType(DataType.Date)]
        public DateTime DateOfBirth { get; set; } = default;
        public string? Gender { get; set; }
        public string? AvatarImage { get; set; }
        [RegularExpression(@"^[^@\s]+@[^@\s]+\.[^@\s]+$", ErrorMessage = "Email Phải Chứa Ký Tự '@'")]
        public string? Email { get; set; } = default;
        public string? Address { get; set; }

    }
}
