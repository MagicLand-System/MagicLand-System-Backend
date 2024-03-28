using System.ComponentModel.DataAnnotations;

namespace MagicLand_System.PayLoad.Request.Course
{
    public class SessionContentRequest
    {
        [Required(ErrorMessage = "Tên Nội Dung Buổi Học Một Số Chỗ Đang Trống")]
        [MaxLength(300, ErrorMessage = "Tên Nội Dung Buổi Học Không Nên Vượt Quá 300 Ký Tự")]
        [MinLength(1, ErrorMessage = "Tên Nội Dung Buổi Học Nên Có Ít Nhất 1 Ký Tự")]
        public required string Content {  get; set; }
        [Required(ErrorMessage = "Nội Dung Chi Tiết Buổi Học Một Số Chỗ Đang Trống")]
        [MaxLength(10, ErrorMessage = "Nội Dung Chi Tiết Buổi Học Không Nên Vượt Quá 10 Nội Dung")]
        [MinLength(1, ErrorMessage = "Nội Dung Chi Tiết Buổi Học Nên Có Ít Nhất 1 Nội Dung")]
        public required List<string> SessionContentDetails {  get; set; }  
    }
}
