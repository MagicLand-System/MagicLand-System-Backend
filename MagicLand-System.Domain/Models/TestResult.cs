using System.ComponentModel.DataAnnotations.Schema;

namespace MagicLand_System.Domain.Models
{
    public class TestResult
    {
        public Guid Id { get; set; }
        public Guid ExamId { get; set; }
        public string? ExamName { get; set; } = string.Empty;
        public string? QuizCategory { get; set; } = string.Empty;
        public string? QuizType { get; set; } = string.Empty;
        public string? QuizName { get; set; } = string.Empty;
        public int TotalMark { get; set; }
        public int CorrectMark { get; set; }
        public TimeSpan DoingTime { get; set; }
        public DateTime DoingDate { get; set; }
        public double TotalScore { get; set; }
        public double ScoreEarned { get; set; }
        public string? ExamStatus { get; set; }
        public int NoAttempt { get; set; }


        [ForeignKey("StudentClass")]
        public Guid StudentClassId { get; set; }
        public StudentClass? StudentClass { get; set; }

        public ICollection<ExamQuestion> ExamQuestions { get; set; } = new List<ExamQuestion>();

    }
}
