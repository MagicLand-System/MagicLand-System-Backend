using System.ComponentModel.DataAnnotations;

namespace MagicLand_System.PayLoad.Request.User
{
    public class UserAccountRequest
    {

        [Required(ErrorMessage = "Tên Người Dùng Đang Trống")]
        [MaxLength(100, ErrorMessage = "Tên Người Dùng Không Được Vượt Quá 100 Ký Tự")]
        [MinLength(10, ErrorMessage = "Tên Người Dùng Phải Có Ít Nhất 10 Ký Tự")]
        public required string UserName { get; set; }
        [Required(ErrorMessage = "Số Điện Thoại Người Dùng Đang Trống")]
        [MaxLength(12, ErrorMessage = "Số Điện Thoại Người Dùng Không Được Vượt Quá 12 Ký Tự")]
        [MinLength(12, ErrorMessage = "Số Điện Thoại Người Dùng Phải Có Ít Nhất 12 Ký Tự")]
        public required string UserPhone { get; set; }
        [Required(ErrorMessage = "Chức Vụ Người Dùng Đang Trống")]
        public required string Role { get; set; }
        public Guid? LecturerCareerId { get; set; }
    }
}
