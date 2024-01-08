namespace MagicLand_System.PayLoad.Response.Course
{
    public class CourseDetailResponse
    {
        public string? CourseName { get; set; } = "Undefined";
        public string? RangeAge { get; set; } = "Undefined";
        public string? Subject { get; set; } = "Undefined";
        public string? Method { get; set; } = "Online";
        public int? NumberOfSession { get; set; }
        public List<string>? CoursePrerequisites { get; set; } = new List<string>();

    }
}
