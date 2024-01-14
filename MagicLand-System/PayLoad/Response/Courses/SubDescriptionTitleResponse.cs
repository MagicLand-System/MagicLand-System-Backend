namespace MagicLand_System.PayLoad.Response.Courses
{
    public class SubDescriptionTitleResponse
    {
        public string? Title { get; set; }
        
        public List<SubDescriptionContentResponse> Contents { get; set; } = new List<SubDescriptionContentResponse>();
    }
}
