using System.ComponentModel.DataAnnotations;

namespace MagicLand_System.PayLoad.Request.Course
{
    public class MutipleChoiceAnswerRequest
    {
        [Required(ErrorMessage = "Nội Dung Câu Trả Lời Không Được Để Trống")]
        [MaxLength(150, ErrorMessage = "Nội Dung Câu Trả Lời Không Nên Vượt Quá 150 Ký Tự")]
        [MinLength(1, ErrorMessage = "Nội Dung Câu Trả Lời Nên Có Ít Nhất 1 Ký Tự")]
        public required string Description { get; set; }
        public string? Img { get; set; }
        public double Score { get; set; } = 0;

    }
}
