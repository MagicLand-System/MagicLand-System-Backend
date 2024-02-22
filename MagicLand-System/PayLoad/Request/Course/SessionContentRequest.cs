namespace MagicLand_System.PayLoad.Request.Course
{
    public class SessionContentRequest
    {
        public required string Content {  get; set; }    
        public required List<string> SessionContentDetails {  get; set; }  
    }
}
