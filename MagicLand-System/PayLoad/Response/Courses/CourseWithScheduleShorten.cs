using MagicLand_System.PayLoad.Response.Schedules;

namespace MagicLand_System.PayLoad.Response.Courses
{
    public class CourseWithScheduleShorten : CourseResponse
    {
        public List<ClassOpeningInfor> ClassOpeningInfors { get; set; } = new List<ClassOpeningInfor>();
        public List<RelatedCourseResponse> RelatedCourses { get; set; } = new List<RelatedCourseResponse>();
        public string? Status { get; set; }
        public int NumberClassOnGoing { get; set; }
        public DateTime? UpdateDate { get; set; }
    }
}
