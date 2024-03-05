using MagicLand_System.PayLoad.Response.Courses;
using MagicLand_System.PayLoad.Response.Sessions;

namespace MagicLand_System.PayLoad.Response.Syllabuses
{
    public class StaffSyllabusResponse
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
        public List<string> ? PreRequisite {  get; set; }   
        public int NumOfSessions {  get; set; }   

        public List<StaffSessionResponse>? SessionResponses { get; set; } = new List<StaffSessionResponse>();
        public List<StaffMaterialResponse>? Materials { get; set; } = new List<StaffMaterialResponse>();
        public List<StaffQuestionPackageResponse>? QuestionPackages { get; set; } = new List<StaffQuestionPackageResponse>();
        public List<StaffExamSyllabusResponse>? Exams { get; set; } = new List<StaffExamSyllabusResponse>();
        public LinkedCourse? LinkedCourse { get; set; }
    }
}
