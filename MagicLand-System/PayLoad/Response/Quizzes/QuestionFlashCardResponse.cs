namespace MagicLand_System.PayLoad.Response.Quizzes
{
    public class QuestionFlashCardResponse : QuestionResponse
    {
        public string? CardQuestion { get; set; } = string.Empty;
        public string? CardAnswer { get; set; } = string.Empty;
        public double Score { get; set; }
    }
}
