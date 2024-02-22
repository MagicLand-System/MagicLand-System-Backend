using MagicLand_System.Domain.Models;

namespace MagicLand_System.PayLoad.Request.Course
{
    public class SyllabusRequest
    {
        public int Index {  get; set; } 
        public string TopicName { get; set; }    
        public List<SessionRequest> SessionRequests { get; set; }
    }
}
