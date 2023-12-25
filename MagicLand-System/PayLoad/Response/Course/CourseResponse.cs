using MagicLand_System.Domain.Models;
using MagicLand_System.PayLoad.Response.Session;
using MagicLand_System.PayLoad.Response.Syllabus;

namespace MagicLand_System.PayLoad.Response.Course
{
    public class CourseResponse
    {
        public Guid Id { get; set; }
        public string? Name { get; set; }
        public string? Subject { get; set; }
        public int? NumberOfSession { get; set; }
        public int? MinAgeStudent { get; set; }
        public int? MaxAgeStudent { get; set; }
        public string? Image { get; set; }
        public decimal? Price { get; set; }
        public string? MainDescription { get; set; }

        public List<SubDescriptionTitleResponse>? SubDescriptionTitle { get; set; }
        public List<CourseResponse>? CoursePrerequisites { get; set; }
        public SyllabusResponse Syllabus { get; set; } = new SyllabusResponse();
    }
}
