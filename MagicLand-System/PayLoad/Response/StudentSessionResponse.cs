using MagicLand_System.PayLoad.Response.Courses;
using MagicLand_System.PayLoad.Response.Courses.Custom;
using MagicLand_System.PayLoad.Response.Sessions;

namespace MagicLand_System.PayLoad.Response
{
    public class StudentSessionResponse
    {
        public string CourseName {  get; set; } 
        public string ClassCode { get; set; }
        public List<SessionContentReponse> Contents { get; set; } = new List<SessionContentReponse>();
      
    }
}
