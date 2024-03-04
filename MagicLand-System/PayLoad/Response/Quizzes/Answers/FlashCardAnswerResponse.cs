namespace MagicLand_System.PayLoad.Response.Quizzes.Answers
{
    public class FlashCardAnswerResponse
    {
        public Guid CardId { get; set; }
        public string? CardDescription { get; set; } = string.Empty;
        public string? CardImage { get; set; } = string.Empty;
        public int NumberCoupleIdentify { get; set; }
        public double Score { get; set; }
    }
}
