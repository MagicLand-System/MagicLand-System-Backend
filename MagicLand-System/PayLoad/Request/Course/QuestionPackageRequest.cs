namespace MagicLand_System.PayLoad.Request.Course
{
    public class QuestionPackageRequest
    {
        public string ContentName { get; set; }
        public int? DeadLine { get; set; }
        public int Duration { get; set; }
        public int? AttemptsAllowed { get; set; }
        public int NoOfSession { get; set; }    
        public string Type { get; set; }
        public string Title { get; set; }
        public int Score { get; set; }
        public List<QuestionRequest> QuestionRequests {  get; set; }    

    }
}
