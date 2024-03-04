using MagicLand_System.PayLoad.Response.Schedules;

namespace MagicLand_System.PayLoad.Response.Courses
{
    public class CourseResponse
    {
        public Guid CourseId { get; set; }
        public bool? IsInCart { get; set; }
        public string? Image { get; set; }
        public decimal? Price { get; set; }
        public string? MainDescription { get; set; }

        public List<SubDescriptionTitleResponse>? SubDescriptionTitle { get; set; }
        public CourseDetailResponse? CourseDetail { get; set; } = new CourseDetailResponse();
    }
}
