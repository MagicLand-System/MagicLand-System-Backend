namespace MagicLand_System.Domain.Models
{
    public class Syllabus
    {
        public Guid Id { get; set; }
        public string? Name { get; set; }
        public DateTime UpdateTime { get; set; }
        public DateTime? EffectiveDate { get; set; } = default;
        public string? StudentTasks { get; set; }
        public double ScoringScale { get; set; }
        public int TimePerSession { get; set; }
        public double MinAvgMarkToPass { get; set; }
        public string? Description { get; set; }
        public string? SyllabusLink { get; set; }
        public string? SubjectCode { get; set; }
        public int? NumOfSessions { get; set; }
        public Guid? PrequisiteSyllabusId { get; set; }


        public Course? Course { get; set; }

        public Guid SyllabusCategoryId { get; set; }
        public SyllabusCategory? SyllabusCategory { get; set; }

        public ICollection<Topic>? Topics { get; set; }
        public ICollection<Material>? Materials { get; set; }
        public ICollection<ExamSyllabus>? ExamSyllabuses { get; set; }
    }
}
