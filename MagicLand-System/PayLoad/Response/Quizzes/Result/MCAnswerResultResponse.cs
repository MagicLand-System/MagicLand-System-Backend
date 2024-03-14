namespace MagicLand_System.PayLoad.Response.Quizzes.Result
{
    public class MCAnswerResultResponse
    {
        public Guid StudentAnswerId { get; set; }
        public string? StudentAnswerDescription { get; set; } = string.Empty;
        public string? StudentAnswerImage { get; set; } = string.Empty;
        public Guid CorrectAnswerId { get; set; }
        public string? CorrectAnswerDescription { get; set;} = string.Empty;
        public string? CorrectAnswerImage { get; set; } = string.Empty;
        public string? Status { get; set; } = string.Empty;
        public double Score { get; set;}
     }
}
