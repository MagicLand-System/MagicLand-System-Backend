using MagicLand_System.Domain.Models;
using System.ComponentModel.DataAnnotations;

namespace MagicLand_System.PayLoad.Request.Course
{
    public class QuestionRequest
    {
        [Required(ErrorMessage = "Nội Dung Câu Hỏi Không Được Để Trống")]
        [MaxLength(150, ErrorMessage = "Nội Dung Câu Hỏi Không Nên Vượt Quá 150 Ký Tự")]
        [MinLength(1, ErrorMessage = "Nội Dung Câu Hỏi Nên Có Ít Nhất 1 Ký Tự")]
        public required string Description { get; set; }
        public string? Img { get; set; }
        public List<MutipleChoiceAnswerRequest>? MutipleChoiceAnswerRequests { get; set; }
        public List<FlashCardRequest>? FlashCardRequests { get; set; } 
    }
}
