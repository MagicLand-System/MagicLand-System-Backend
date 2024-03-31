using MagicLand_System.PayLoad.Response.Schedules;

namespace MagicLand_System.PayLoad.Response.Courses.Custom
{
    public class CourseResponseCustom
    {
        public Guid CourseId { get; set; }
        public bool? IsInCart { get; set; }
        public Guid? CartItemId { get; set; }
        public string? Image { get; set; }
        public double? Price { get; set; }
        public string? MainDescription { get; set; }
        public string? CourseName { get; set; } = "Undefined";
        public string? SubjectCode { get; set; } = "Undefined";
        public string? MinAgeStudent { get; set; } = "Undefined";
        public string? MaxAgeStudent { get; set; } = "Undefined";
        public string? Subject { get; set; } = "Undefined";
        public string? Method { get; set; } = "Online";
        public int? NumberOfSession { get; set; }
        public List<string>? CoursePrerequisites { get; set; } = new List<string>();
        public List<SubDescriptionTitleResponse>? SubDescriptionTitle { get; set; }
        public List<OpeningScheduleResponse> OpeningSchedules { get; set; } = new List<OpeningScheduleResponse>();
        public List<RelatedCourseResponse> RelatedCourses { get; set; } = new List<RelatedCourseResponse>();
        public DateTime? AddedDate { get; set; }
        public int NumberClassOnGoing { get; set; }
        public DateTime? UpdateDate { get; set; }
    }
}
