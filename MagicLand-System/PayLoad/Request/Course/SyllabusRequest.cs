using MagicLand_System.Domain.Models;
using System.ComponentModel.DataAnnotations;

namespace MagicLand_System.PayLoad.Request.Course
{
    public class SyllabusRequest
    {
        [Required(ErrorMessage = "Thứ Tự Chủ Đề Một Số Chỗ Đang Trống")]
        [Range(0, 100, ErrorMessage = "Thứ Tự Chủ Đề Một Số Chỗ Đang Không Hợp Lệ [1-99]")]
        public required int Index {  get; set; }
        [Required(ErrorMessage = "Tên Chủ Đề Một Số Chỗ Đang Trống")]
        [MinLength(1, ErrorMessage = "Tên Chủ Đề Nên Có Ít Nhất 1 Ký Tự")]
        public required string TopicName { get; set; }
        [Required(ErrorMessage = "Nội Dung Buổi Học Của Chủ Đề Một Số Chỗ Đang Trống")]
        [MaxLength(5, ErrorMessage = "Nội Dung Buổi Học Của Chủ Đề Một Số Không Nên Vượt Quá 5 Nội Dung")]
        [MinLength(1, ErrorMessage = "Nội Dung Buổi Học Của Chủ Đề Một Số Nên Có Ít Nhất 1 Nội Dung")]
        public required List<SessionRequest> SessionRequests { get; set; }
    }
}
