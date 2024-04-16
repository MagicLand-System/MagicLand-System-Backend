namespace MagicLand_System.PayLoad.Response.Quizzes.Result
{
    public class QuizResultExtraInforResponse : QuizResultResponse
    {
        public Guid ResultId { get; set; }
        public Guid ExamId { get; set; }
        public int NoAttempt { get; set; }
        public string? ExamName { get; set; } = string.Empty;
        public string? QuizName { get; set; } = string.Empty;
        public string? QuizCategory { get; set; } = string.Empty;
        public string? QuizType { get; set; } = string.Empty;

    }
}
