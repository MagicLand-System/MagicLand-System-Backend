namespace MagicLand_System.PayLoad.Request.Course
{
    public class SubDescriptionRequest
    {
        public string Title {  get; set; }  
        public List<SubDescriptionContentRequest> SubDescriptionContentRequests { get; set; }
    }
}
