using MagicLand_System.PayLoad.Response.Courses;
using MagicLand_System.PayLoad.Response.Sessions;
using MagicLand_System.PayLoad.Response.Topics;

namespace MagicLand_System.PayLoad.Response.Syllabuses
{
    public class SyllabusResponse
    {
        public CourseSimpleResponse? Course { get; set; } = new CourseSimpleResponse();
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
        public DateTime UpdateDate { get; set; }
        public int NumberOfSyllabuses {get; set; }  
        public List<MaterialResponse>? Materials { get; set; } = new List<MaterialResponse>();
        public SyllabusInforResponse? SyllabusInformations { get; set; } = new SyllabusInforResponse();
        public List<ExamSyllabusResponse>? Exams { get; set; } = new List<ExamSyllabusResponse>();
        public List<QuestionPackageResponse>? QuestionPackages { get; set; } = new List<QuestionPackageResponse>();
    }
}
