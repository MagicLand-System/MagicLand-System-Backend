namespace MagicLand_System.PayLoad.Response.Quizzes.Result
{
    public class QuizResultResponse
    {
        public required int TotalMark { get; set; }
        public required int CorrectMark { get; set; }
        public required double TotalScore { get; set; }
        public required double ScoreEarned { get; set; }
        public required string ExamStatus { get; set; }
    }
}
