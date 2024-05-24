using System.ComponentModel.DataAnnotations.Schema;

namespace MagicLand_System.Domain.Models
{
    public class ExamQuestion
    {
        public Guid Id { get; set; }
        public Guid QuestionId { get; set; }
        public string? Question { get; set; } = string.Empty;
        public string? QuestionImage { get; set; } = string.Empty;


        [ForeignKey("ExamResult")]
        public Guid ExamResultResultId { get; set; }
        public ExamResult? ExamResult { get; set; }

        [ForeignKey("MultipleChoiceAnswer")]
        public Guid? MultipleChoiceAnswerId { get; set; }
        public MultipleChoiceAnswer? MultipleChoiceAnswer { get; set; }

        public ICollection<FlashCardAnswer>? FlashCardAnswers { get; set; }
    }
}
