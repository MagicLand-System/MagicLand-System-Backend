namespace MagicLand_System.PayLoad.Request.Course
{
    public class QuestionPackageRequest
    {
        public int NoOfSession { get; set; }    
        public string Type { get; set; }
        public string Title { get; set; }
        public List<QuestionRequest> QuestionRequests {  get; set; }    

    }
}
