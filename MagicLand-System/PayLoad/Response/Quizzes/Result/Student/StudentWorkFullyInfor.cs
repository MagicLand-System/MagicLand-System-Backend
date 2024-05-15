namespace MagicLand_System.PayLoad.Response.Quizzes.Result.Student
{
    public class StudentWorkFullyInfor
    {
        public required Guid ExamId { get; set; }
        public required string ExamName { get; set; } = string.Empty;
        public required int NoAttempt { get; set; }
        public required string QuizCategory { get; set; } = string.Empty;
        public required string QuizType { get; set; } = string.Empty;
        public required string QuizName { get; set; } = string.Empty;
        public required int? TotalMark { get; set; }
        public required int? CorrectMark { get; set; }
        public required double? TotalScore { get; set; }
        public required double? ScoreEarned { get; set; }
        public required TimeSpan? DoingTime { get; set; }
        public DateTime? DoingDate { get; set; }
        public required string? ExamStatus { get; set; }
        public double? Weight { get; set; }
        public List<StudentWorkResult>? StudentWorkResult { get; set; }
    }
}
