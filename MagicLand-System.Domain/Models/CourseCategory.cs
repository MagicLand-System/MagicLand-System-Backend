namespace MagicLand_System.Domain.Models
{
    public class CourseCategory
    {
        public Guid Id { get; set; }
        public string? Name { get; set; } 
        public ICollection<CourseSyllabus> CourseSyllabuses { get; set; } = new List<CourseSyllabus>();
    }
}
