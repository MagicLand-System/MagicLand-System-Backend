namespace MagicLand_System.PayLoad.Response.Quizzes
{
    public class QuestionMutipleChoiceResponse : QuestionResponse
    {
        public string? Answer { get; set; } = string.Empty;
        public string? AnswerImage { get; set; } = string.Empty;
        public double Score { get; set; } = 0;

    }
}
