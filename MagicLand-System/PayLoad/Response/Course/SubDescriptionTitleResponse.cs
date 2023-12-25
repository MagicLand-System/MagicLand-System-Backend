namespace MagicLand_System.PayLoad.Response.Course
{
    public class SubDescriptionTitleResponse
    {
        public string? Title { get; set; }
        
        public List<SubDescriptionContentResponse> Contents { get; set; } = new List<SubDescriptionContentResponse>();
    }
}
