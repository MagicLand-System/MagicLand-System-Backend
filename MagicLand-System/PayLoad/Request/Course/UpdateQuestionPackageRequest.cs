namespace MagicLand_System.PayLoad.Request.Course
{
    public class UpdateQuestionPackageRequest
    {
        public string? ContentName { get; set; }
        public string? Type { get; set; }
        public string? Title { get; set; }
        public int?  Score { get; set; }
        public List<QuestionRequest> QuestionRequests { get; set; }
    }
}
