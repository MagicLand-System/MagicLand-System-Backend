using MagicLand_System.PayLoad.Response.Courses;
using MagicLand_System.PayLoad.Response.Courses.Custom;
using MagicLand_System.PayLoad.Response.Sessions;

namespace MagicLand_System.PayLoad.Response
{
    public class StudentSessionResponse
    {
        public int Index {  get; set; } 
        public DateTime Date { get; set; }
        public string StartTime {  get; set; }
        public string EndTime { get; set; }
        public string TopicName {  get; set; }  
        public string CourseName {  get; set; } 
        public string ClassCode { get; set; }
        public List<SessionContentReponse> Contents { get; set; } = new List<SessionContentReponse>();
      
    }
}
