namespace MagicLand_System.PayLoad.Response
{
    public class CourseResponse
    {
        public Guid Id { get; set; }
        public string? Name { get; set; }
        public int? NumberOfSession { get; set; } 
        public int? MinAgeStudent { get; set; }
        public int? MaxAgeStudent { get; set; }


        public List<CourseResponse>? CoursePrerequisites { get; set; }
    }
}
