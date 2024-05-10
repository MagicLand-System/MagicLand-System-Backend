using MagicLand_System.PayLoad.Response.Students;
using MagicLand_System.PayLoad.Response.Users;

namespace MagicLand_System.PayLoad.Response
{
    public class SaveCourseResponse
    {
        public Guid Id { get; set; }    
        public StudentResponse StudentResponse { get; set; }
        public UserResponse ParentResponse { get; set; }
        public string CourseName { get; set; }
        public string Status { get; set; }
        public DateTime? SavedTime { get; set; }
        public Guid CourseId { get; set; }  
    }
}
