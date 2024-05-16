using System.ComponentModel.DataAnnotations;

namespace MagicLand_System.PayLoad.Request.User
{

    public class UserRequest
    {
        [MaxLength(100, ErrorMessage = "Tên Người Dùng Không Được Vượt Quá 100 Ký Tự")]
        [MinLength(3, ErrorMessage = "Tên Người Dùng Phải Có Ít Nhất 3 Ký Tự")]
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
