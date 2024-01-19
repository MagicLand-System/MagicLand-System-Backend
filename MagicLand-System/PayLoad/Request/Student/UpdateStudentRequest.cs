using System.ComponentModel.DataAnnotations;

namespace MagicLand_System.PayLoad.Request.Student
{
    public class UpdateStudentRequest 
    {
        public required Guid StudentId { get; set; }
        [RegularExpression("^[A-Z][a-z]*( [A-Z][a-z]*)*$", ErrorMessage = "FullName Cần Phải Viết Hoa Đầu Mỗi Ký Chữ")]
        public string? FullName { get; set; }
        [DataType(DataType.Date)]
        public DateTime DateOfBirth { get; set; } = default;
        public string? Gender { get; set; }
        public string? AvatarImage { get; set; }
        [RegularExpression(@"^[^@\s]+@[^@\s]+\.[^@\s]+$", ErrorMessage = "Email Phải Chứa Ký Tự '@'")]
        public string? Email { get; set; }

    }
}
