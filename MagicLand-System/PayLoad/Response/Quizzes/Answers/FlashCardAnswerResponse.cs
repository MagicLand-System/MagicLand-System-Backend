namespace MagicLand_System.PayLoad.Response.Quizzes.Answers
{
    public class FlashCardAnswerResponse
    {
        public Guid FirstCardId { get; set; }
        public string? FirstCardInfor { get; set; } = string.Empty;
        public Guid SecondCardId { get; set; }
        public string? SecondCardInfor { get; set; } = string.Empty;
        public double Score { get; set; }
    }
}
