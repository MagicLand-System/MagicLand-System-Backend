using System.ComponentModel.DataAnnotations;

namespace MagicLand_System.PayLoad.Request.Course
{
    public class OverallSyllabusRequest
    {
        [Required(ErrorMessage = "Tên Giáo Trình Không Được Để Trống")]
        [MinLength(5, ErrorMessage = "Tên Giáo Trình Nên Có Ít Nhất 5 Ký Tự")]
        public required string SyllabusName { get; set; }
        [Required(ErrorMessage = "Ngày Hiệu Lực Không Được Để Trống")]
        public required string EffectiveDate { get; set; }
        public string? StudentTasks { get; set; }
        public double? ScoringScale { get; set; } = 10.0;
        [Required(ErrorMessage = "Thời Gian Mỗi Buổi Học Không Được Để Trống")]
        [Range(1, 300, ErrorMessage = "Thời Gian Mỗi Buổi Học Không Hợp Lệ [1-300]")]
        public required int TimePerSession { get; set; }
        public double? MinAvgMarkToPass { get; set; } = 5.0;
        [MinLength(1, ErrorMessage = "Mô Tả Giáo Trình Không Nên Có Ít Nhất 1 Ký Tự")]
        public string? Description { get; set; } = string.Empty;
        [Required(ErrorMessage = "Mã Giáo Trình Không Được Để Trống")]
        public required string SubjectCode { get; set; }
        [Required(ErrorMessage = "Đường Dẫn Giáo Trình Không Được Để Trống")]
        public required string SyllabusLink { get; set; }
        public List<string>? PreRequisite { get; set; }
        [Required(ErrorMessage = "Môn Học Giáo Trình Không Được Để Trống")]
        public required string Type { get; set; }
        [Required(ErrorMessage = "Tổng Số Buổi Học Của Giáo Trình Không Được Để Trống")]
        [Range(1, 31, ErrorMessage = "Tổng Số Buổi Học Của Giáo Trình Phải Lớn Hơn 0 Và Nhỏ Hơn Hoặc Bằng 30")]
        public required int NumOfSessions { get; set; }

        [Required(ErrorMessage = "Nội Dung Giáo Trình Không Được Để Trống")]
        [MaxLength(25, ErrorMessage = "Nội Dung Giáo Trình Không Nên Vượt Quá 25 Nội Dung")]
        [MinLength(1, ErrorMessage = "Nội Dung Giáo Trình Nên Có Ít Nhất 1 Nội Dung")]
        public required List<SyllabusRequest> SyllabusRequests { get; set; }
        [Required(ErrorMessage = "Tài Nguyên Giáo Trình Không Được Để Trống")]
        [MaxLength(10, ErrorMessage = "Tài Nguyên Giáo Trình Không Nên Vượt Quá 10 Tài Nguyên")]
        [MinLength(1, ErrorMessage = "Tài Nguyên Giáo Trình Nên Có Ít Nhất 1 Tài Nguyên")]
        public required List<MaterialRequest> MaterialRequests { get; set; }
        [Required(ErrorMessage = "Thông Tin Các Bài Kiểm Tra Của Giáo Trình Đang Trống")]
        [MaxLength(10, ErrorMessage = "Các Bài Kiểm Tra Giáo Trình Không Nên Vượt Quá 10 Bài")]
        [MinLength(3, ErrorMessage = "Các Bài Kiểm Tra Giáo Trình Nên Có Ít Nhất 3 Bài")]
        public required List<ExamSyllabusRequest> ExamSyllabusRequests { get; set; }
        [Required(ErrorMessage = "Thông Tin Bộ Đề Câu Hỏi Và Câu Trả Lời Của Giáo Trình Đang Trống")]
        [MaxLength(15, ErrorMessage = "Bộ Đề Câu Hỏi Và Câu Trả Lời Không Nên Vượt Quá 15 Bộ Đề")]
        [MinLength(3, ErrorMessage = "Bộ Đề Câu Hỏi Và Câu Trả Lời Nên Có Ít Nhất 3 Bộ Đề")]
        public required List<QuestionPackageRequest> QuestionPackageRequests { get; set; }
    }
}
