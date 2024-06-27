using MagicLand_System.PayLoad.Response.Courses;

namespace MagicLand_System.PayLoad.Response.Syllabuses.ForStaff
{
    public class GeneralSyllabusResponse
    {
        public Guid SyllabusId { get; set; }
        public string? SyllabusName { get; set; } = string.Empty;
        public string? Category { get; set; } = string.Empty;
        public string? EffectiveDate { get; set; } = string.Empty;
        public string? StudentTasks { get; set; } = string.Empty;
        public double ScoringScale { get; set; }
        public int TimePerSession { get; set; }
        public int SessionsPerCourse { get; set; }
        public double MinAvgMarkToPass { get; set; }
        public string? Description { get; set; } = string.Empty;
        public string? SubjectCode { get; set; } = string.Empty;
        public string? SyllabusLink { get; set; } = string.Empty;
        public List<string>? PreRequisite { get; set; }
        public LinkedCourse? LinkedCourse { get; set; }
    }
}
