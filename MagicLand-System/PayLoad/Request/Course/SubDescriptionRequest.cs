namespace MagicLand_System.PayLoad.Request.Course
{
    public class SubDescriptionRequest
    {
        public string? Title { get; set; } = string.Empty;
        public List<SubDescriptionContentRequest>? SubDescriptionContentRequests { get; set; }
    }
}
