namespace MagicLand_System.PayLoad.Response.Quizzes.Staff
{
    public class StaffMultipleChoiceResponse
    {
        public Guid MultipleChoiceId { get; set; }
        public string? Answer { get; set; } = string.Empty;
        public string? AnswerImage { get; set; } = string.Empty;
        public double Score { get; set; } = 0;
    }
}
