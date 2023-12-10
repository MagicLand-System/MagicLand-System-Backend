namespace MagicLand_System.Domain.Models
{
    public class Course
    {
        public Guid Id { get; set; }
        public string? Name { get; set; }
        public int NumberOfSession { get; set; }
        public int MinYearOldsStudent { get; set; } = 3;
        public int MaxYearOldsStudent { get; set; } = 120;
        public string? Status { get; set; }
        public string? Image { get; set; } = null;


        public ICollection<Session> Sessions { get; set; } = new List<Session>();
        public ICollection<CoursePrerequisite> CoursePrerequisites { get; set; } = new List<CoursePrerequisite>();
        public ICollection<Class> Classes { get; set; } = new List<Class>();
    }
}
