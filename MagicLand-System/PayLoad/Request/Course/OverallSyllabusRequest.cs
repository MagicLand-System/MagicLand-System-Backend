using MagicLand_System.Domain.Models;

namespace MagicLand_System.PayLoad.Request.Course
{
    public class OverallSyllabusRequest
    {
        public string? SyllabusName { get; set; }
        public string? EffectiveDate { get; set; } 
        public string? StudentTasks { get; set; }
        public double ScoringScale { get; set; }
        public int TimePerSession { get; set; }
        public double MinAvgMarkToPass { get; set; }
        public string? Description { get; set; }
        public string? SubjectCode {  get; set; }   
        public string? SyllabusLink {  get; set; }  
        public List<string>? PreRequisite { get; set; } 
        public string? Type { get; set; }


        public List<SyllabusRequest> SyllabusRequests { get; set; } = new List<SyllabusRequest>();
        public List<string> MaterialRequests { get; set; } = new List<string>();
        public List<ExamSyllabusRequest> ExamSyllabusRequests { get; set; } = new List<ExamSyllabusRequest>(); 
        public List<QuestionPackageRequest>? QuestionPackageRequests { get; set; }
    }
}
