namespace MagicLand_System.PayLoad.Response.Quizzes.Result.Final
{
    public class FinalTestResultResponse
    {
        //public required int Order { get; set; }
        public Guid ExamId { get; set; }
        public string? QuizName { get; set; } = string.Empty;
        public string? QuizType { get; set; } = string.Empty;
        public string? QuizCategory { get; set; } = string.Empty;
        public double Weight { get; set; }
        public double Score { get; set; }
        public double ScoreWeight { get; set; }
    }
}
