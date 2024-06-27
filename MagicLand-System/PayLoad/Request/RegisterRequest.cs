using System.ComponentModel.DataAnnotations;

namespace MagicLand_System.PayLoad.Request
{
    public class RegisterRequest
    {
        [Required(ErrorMessage = "Tên Phụ Huynh Đang Trống")]
        [MaxLength(100, ErrorMessage = "Tên Phụ Huynh Không Được Vượt Quá 100 Ký Tự")]
        public required string FullName { get; set; }
        [Required(ErrorMessage = "Số Điện Thoại Phụ Huynh Đang Trống")]
        public required string Phone { get; set; }
        [EmailAddress(ErrorMessage = "Định Dạng Email Không Hợp Lệ")]
        public string? Email { get; set; }
        public string? Gender { get; set; }
        public DateTime? DateOfBirth { get; set; } = null;
        [Required(ErrorMessage = "Địa Chỉ Đang Trống")] 
        public required string Address {  get; set; }   

    }
}
