using MagicLand_System.Domain.Models;

namespace MagicLand_System.PayLoad.Request.Course
{
    public class OverallSyllabusRequest
    {
        public string? Name { get; set; }
        public DateTime? EffectiveDate { get; set; } = default;
        public string? StudentTasks { get; set; }
        public double ScoringScale { get; set; }
  
        public int TimePerSession { get; set; }
        public double MinAvgMarkToPass { get; set; }
        public string? Description { get; set; }
        public string? SubjectCode {  get; set; }   
        public string? SyllabusLink {  get; set; }  
        public List<SyllabusRequest> SyllabusRequests { get; set; } = new List<SyllabusRequest>();
        public List<string> MaterialRequests { get; set; } = new List<string>();
        public List<ExamSyllabusRequest> ExamSyllabusRequests { get; set; } = new List<ExamSyllabusRequest>(); 
        public List<QuestionPackageRequest> QuestionPackageRequests { get; set; }

    }
}
