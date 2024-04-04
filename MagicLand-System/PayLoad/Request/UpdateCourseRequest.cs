namespace MagicLand_System.PayLoad.Request
{
    public class UpdateCourseRequest
    {
        public string? CourseName { get; set; }
        public double? Price { get; set; }
        public int? MinAge { get; set; }
        public int? MaxAge { get; set; }
        public string? MainDescription { get; set; }
        public string? Img { get; set; }
    }
}
