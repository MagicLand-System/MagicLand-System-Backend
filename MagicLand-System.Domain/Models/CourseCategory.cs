namespace MagicLand_System.Domain.Models
{
    public class CourseCategory
    {
        public Guid Id { get; set; }
        public string? Name { get; set; } = "MATH";


        public ICollection<Course> Courses { get; set; } = new List<Course>();
    }
}
