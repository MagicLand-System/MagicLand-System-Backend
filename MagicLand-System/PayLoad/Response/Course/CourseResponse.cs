using MagicLand_System.Domain.Models;
using MagicLand_System.PayLoad.Response.Schedule;
using MagicLand_System.PayLoad.Response.Session;
using MagicLand_System.PayLoad.Response.Syllabus;

namespace MagicLand_System.PayLoad.Response.Course
{
    public class CourseResponse
    {
        public Guid CourseId { get; set; }   
        public string? Image { get; set; }
        public decimal? Price { get; set; }
        public string? MainDescription { get; set; }

        public List<SubDescriptionTitleResponse>? SubDescriptionTitle { get; set; }
        public CourseDetailResponse? CourseDetail { get; set; } = new CourseDetailResponse();
        public List<OpeningScheduleResponse> OpeningSchedules { get; set; } = new List<OpeningScheduleResponse>();
        public List<RelatedCourseResponse> RelatedCourses { get; set; } = new List<RelatedCourseResponse>();
    }
}
