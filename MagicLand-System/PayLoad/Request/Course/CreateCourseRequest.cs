using Org.BouncyCastle.Crypto.Engines;

namespace MagicLand_System.PayLoad.Request.Course
{
    public class CreateCourseRequest
    {
        public required string CourseName { get; set; }
        public double Price { get; set; }
        public int MinAge { get; set; } = 4;
        public int MaxAge { get; set; } = 10;
        public string? MainDescription { get; set; } = string.Empty;
        public string? Img { get; set; }
        public required string SyllabusId { get; set; }

        public List<SubDescriptionRequest>? SubDescriptions { get; set; }

    }
}
