using System.ComponentModel.DataAnnotations;

namespace MagicLand_System.PayLoad.Request.Course
{
    public class ExamSyllabusRequest
    {
        [Required(ErrorMessage = "Loại Kiểm Tra Đang Trống")]
        public required string Type { get; set; }
        [Required(ErrorMessage = "Chủ Đề Kiểm Tra Đang Trống")]
        public required string ContentName { get; set; }
        [Required(ErrorMessage = "Trọng Lượng Tổng Bài Kiểm Tra Đang Trống")]
        [Range(1, 101, ErrorMessage = "Trọng Lượng Tổng Bài Kiểm Tra Không Hợp Lệ [1-100]")]
        public required double Weight { get; set; }
        public double? CompleteionCriteria { get; set; } = 0.0;
        public string? QuestionType { get; set; } = string.Empty;
        public int? Part { get; set; } = 1;
        [Required(ErrorMessage = "Hình Thức Kiểm Tra Đang Trống")]
        public required string Method { get; set; }
        public string? Duration { get; set; } = string.Empty;
    }
}
