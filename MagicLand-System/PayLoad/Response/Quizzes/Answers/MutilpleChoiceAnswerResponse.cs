namespace MagicLand_System.PayLoad.Response.Quizzes.Answers
{
    public class MutilpleChoiceAnswerResponse
    {
        public Guid AnswerId { get; set; }
        public string? AnswerDescription { get; set; } = string.Empty;
        public string? AnswerImage { get; set; } = string.Empty;
        public double Score { get; set; } = 0;
    }
}
