namespace MagicLand_System.PayLoad.Response.Courses
{
    public class CourseDetailResponse
    {
        public string? CourseName { get; set; } = "Undefined";
        public string? SubjectCode { get; set; } = "Undefined";
        public string? MinAgeStudent { get; set; } = "Undefined";
        public string? MaxAgeStudent { get; set; } = "Undefined";
        public string? Subject { get; set; } = "Undefined";
        public string? Method { get; set; } = "Online";
        public int? NumberOfSession { get; set; }
        public DateTime? AddedDate { get; set; }

        public List<string>? CoursePrerequisites { get; set; } = new List<string>();

    }
}
