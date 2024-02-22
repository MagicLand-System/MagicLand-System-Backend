namespace MagicLand_System.PayLoad.Request.Course
{
    public class SessionRequest
    {
        public int Order {  get; set; } 
        public List<SessionContentRequest> SessionContentRequests { get; set; }
       
    }
}
