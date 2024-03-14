using System.ComponentModel.DataAnnotations;

namespace MagicLand_System.PayLoad.Request.Quizzes
{
    public class StudentExamGrade
    {
        [Required(ErrorMessage = "Id Của Học Sinh Không Được Trống")]
        public required Guid StudentId { get; set; }
        [Required(ErrorMessage = "Điểm Của Học Sinh Không Được Trống")]
        [Range(0, 10, ErrorMessage = "Điểm Phải Bắt Đầu Từ 0 Đến 10")]
        public required double Score { get; set; }
    }
}
