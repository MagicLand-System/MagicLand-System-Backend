using System.ComponentModel.DataAnnotations.Schema;

namespace MagicLand_System.Domain.Models
{
    public class Course
    {
        public Guid Id { get; set; }
        public string? Name { get; set; }
        public DateTime? AddedDate { get; set; } = default;
        public string? SubjectName { get; set; }
        public int NumberOfSession { get; set; }
        public int? MinYearOldsStudent { get; set; } = 3;
        public int? MaxYearOldsStudent { get; set; } = 120;
        public string? Image { get; set; } = null;
        public double TotalRate { get; set; } = 0.0;
        public string? MainDescription { get; set; }
        public DateTime? UpdateDate { get; set; } = default;


        [ForeignKey("Syllabus")]
        public Guid SyllabusId { get; set; }
        public Syllabus? Syllabus { get; set; }
        public ICollection<Class> Classes { get; set; } = new List<Class>();
        public ICollection<SubDescriptionTitle> SubDescriptionTitles { get; set; } = new List<SubDescriptionTitle>();
        public ICollection<CoursePrice>? CoursePrices { get; set; }
    }
}
