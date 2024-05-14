using System.ComponentModel.DataAnnotations.Schema;

namespace MagicLand_System.Domain.Models
{
    public class MultipleChoiceAnswer
    {
        public Guid Id { get; set; }
        public Guid AnswerId { get; set; }
        public string? Answer { get; set; } = string.Empty;
        public string? AnswerImage { get; set; } = string.Empty;
        public Guid CorrectAnswerId { get; set; }
        public string? CorrectAnswer { get; set; } = string.Empty;
        public string? CorrectAnswerImage { get; set; } = string.Empty;
        public string? Status { get; set; }
        public double Score { get; set; }


        [ForeignKey("ExamQuestion")]
        public Guid ExamQuestionId { get; set; }
        public ExamQuestion? ExamQuestion { get; set; }
    }
}
