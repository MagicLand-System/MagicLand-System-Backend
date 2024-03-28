using MagicLand_System.PayLoad.Response.Quizzes.Questions;

namespace MagicLand_System.PayLoad.Response.Quizes
{
    public class ExamResponse
    {
        public Guid ExamId { get; set; }
        public int ExamPart { get; set; }
        public string? ExamName { get; set; }
        public string? QuizCategory { get; set; } = string.Empty;
        public string? QuizType { get; set; } = string.Empty;
        public string? QuizName { get; set; }
        public double Weight { get; set; }
        public double? CompletionCriteria { get; set; }
        public double TotalScore { get; set; }
        public int TotalMark { get; set; }
        //public int? DeadLine { get; set; }
        public string? Date { get; set; } = string.Empty;
        public DateTime? ExamStartTime { get; set; }
        public DateTime? ExamEndTime { get; set; }
        public int? Duration { get; set; }
        public int? AttemptAlloweds { get; set; }
        public int NoSession { get; set; }
        public Guid SessionId { get; set; }
        public Guid CourseId { get; set; }
    }
}
