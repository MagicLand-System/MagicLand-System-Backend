using MagicLand_System.PayLoad.Response.Schedules;

namespace MagicLand_System.PayLoad.Response.Courses
{
    public class CourseResExtraInforV2 : CourseResponse
    {
        public List<OpeningScheduleResponse> Schedules { get; set; } = new List<OpeningScheduleResponse>();
        public List<RelatedCourseResponse> RelatedCourses { get; set; } = new List<RelatedCourseResponse>();
        public int NumberClassOnGoing { get; set; }
        public DateTime? UpdateDate { get; set; }
    }
}
