using System.ComponentModel.DataAnnotations;

namespace MagicLand_System.PayLoad.Request.Course
{
    public class QuestionPackageRequest
    {
        [Required(ErrorMessage = "Tên Bộ Đề Đang Trống")]
        public required string ContentName { get; set; }
        [Required(ErrorMessage = "Thự Tự Buổi Học Đang Trống")]
        [Range(1, 31, ErrorMessage = "Thự Tự Buổi Học Không Hợp Lệ [1-30]")]
        public required int NoOfSession { get; set; }
        [Required(ErrorMessage = "Loại Bộ Đề Đang Trống")]
        public required string Type { get; set; }
        public string? Title { get; set; } = string.Empty;
        [Required(ErrorMessage = "Điểm Bộ Đề Đang Trống")]
        [Range(1, 10, ErrorMessage = "Điểm Bộ Đề Đang Không Hợp Lệ [1-10]")]
        public required int Score { get; set; }
        public int? Duration { get; set; }
        [Required(ErrorMessage = "Câu Hỏi Và Trả Lời Của Bộ Đề Không Được Để Trống")]
        public required List<QuestionRequest> QuestionRequests { get; set; }

    }
}
