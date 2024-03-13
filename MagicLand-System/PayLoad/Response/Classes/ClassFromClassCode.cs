using MagicLand_System.PayLoad.Response.Courses;
using MagicLand_System.PayLoad.Response.Users;

namespace MagicLand_System.PayLoad.Response.Classes
{
    public class ClassFromClassCode
    {
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string? Status { get; set; }
        public string? Method { get; set; }
        public int LimitNumberStudent { get; set; }
        public int LeastNumberStudent { get; set; }
        public string? Image { get; set; } = null;
        public string? Video { get; set; } = null;
        public DateTime? AddedDate { get; set; }
        public LecturerResponse LecturerResponse { get; set; }
        public StaffCourseResponse StaffCourseResponse { get; set; }    

    }
}
