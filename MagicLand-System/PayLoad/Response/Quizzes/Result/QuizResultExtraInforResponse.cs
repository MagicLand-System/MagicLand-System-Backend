namespace MagicLand_System.PayLoad.Response.Quizzes.Result
{
    public class QuizResultExtraInforResponse : QuizResultResponse
    {
        public Guid ResultId { get; set; }
        public Guid ExamId { get; set; }
        public int NoAttemp { get; set; }
        public string? ExamName { get; set; } = string.Empty;
        public string? ExamCategory { get; set; } = string.Empty;
        public string? ExamType { get; set; } = string.Empty;
    }
}
