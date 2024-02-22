using MagicLand_System.PayLoad.Response.Sessions;
using MagicLand_System.PayLoad.Response.Topics;

namespace MagicLand_System.PayLoad.Response.Syllabuses
{
    public class SyllabusResponse
    {
        public string? SyllabusName { get; set; } = string.Empty;
        public string? EffectiveDate { get; set; } = string.Empty;
        public string? StudentTasks { get; set; } = string.Empty;
        public double ScoringScale { get; set; }
        public int TimePerSession { get; set; }
        public double MinAvgMarkToPass { get; set; }
        public string? Description { get; set; } = string.Empty;
        public string? SubjectCode { get; set; } = string.Empty;
        public string? SyllabusLink { get; set; } = string.Empty;
        public SyllabusInforResponse? SyllabusInformations { get; set; } = new SyllabusInforResponse();
        public MaterialResponse? Materials { get; set; } = new MaterialResponse();
        public QuestionPackageResponse? QuestionPackages { get; set; } = new QuestionPackageResponse();
        public ExamSyllabusResponse? Exams { get; set; } = new ExamSyllabusResponse();
    }
}
