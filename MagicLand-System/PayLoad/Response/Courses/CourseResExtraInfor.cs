using MagicLand_System.PayLoad.Response.Schedules;

namespace MagicLand_System.PayLoad.Response.Courses
{
     public class CourseResExtraInfor : CourseResponse
    {
        public List<OpeningScheduleResponse> OpeningSchedules { get; set; } = new List<OpeningScheduleResponse>();
        public List<RelatedCourseResponse> RelatedCourses { get; set; } = new List<RelatedCourseResponse>();
        public int NumberClassOnGoing {get; set; }

    }
}
