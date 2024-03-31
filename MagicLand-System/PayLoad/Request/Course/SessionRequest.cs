using System.ComponentModel.DataAnnotations;

namespace MagicLand_System.PayLoad.Request.Course
{
    public class SessionRequest
    {
        [Required(ErrorMessage = "Thứ Tự Buổi Học Một Số Chỗ Đang Trống")]
        [Range(0, 100, ErrorMessage = "Thứ Tự Buổi Học Một Số Chỗ Đang Không Hợp Lệ [1-99]")]
        public required int Order {  get; set; }
        [Required(ErrorMessage = "Nội Dung Buổi Học Một Số Chỗ Đang Trống")]
        [MaxLength(5, ErrorMessage = "Nội Dung Buổi Học Một Số Chỗ Không Nên Vượt Quá 5 Nội Dung")]
        [MinLength(1, ErrorMessage = "Nội Dung Buổi Học Một Số Chỗ Nên Có Ít Nhất 1 Nội Dung")]
        public required List<SessionContentRequest> SessionContentRequests { get; set; }
       
    }
}
