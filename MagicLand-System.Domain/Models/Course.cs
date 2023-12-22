using OfficeOpenXml.ConditionalFormatting.Contracts;
using System.ComponentModel.DataAnnotations.Schema;

namespace MagicLand_System.Domain.Models
{
    public class Course
    {
        public Guid Id { get; set; }
        public string? Name { get; set; }
        public int NumberOfSession { get; set; }
        public int? MinYearOldsStudent { get; set; } = 3;
        public int? MaxYearOldsStudent { get; set; } = 120;
        public string? Status { get; set; }
        public string? Image { get; set; } = null;
        public double Price { get; set; }

        [ForeignKey("CourseCategory")]
        public Guid CourseCategoryId { get; set; }
        public Guid? CourseSyllabusId { get; set; }
        public CourseSyllabus? CourseSyllabus { get; set; }
        public CourseCategory CourseCategory { get; set; } = new CourseCategory();
        public ICollection<CoursePrerequisite> CoursePrerequisites { get; set; } = new List<CoursePrerequisite>();
        public ICollection<Class> Classes { get; set; } = new List<Class>();
        public ICollection<CourseDescription> CourseDescriptions { get; set; } = new List<CourseDescription>();
    }
}
