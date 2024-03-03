namespace MagicLand_System.PayLoad.Request.Syllabus
{
    public class UpdateOverallSyllabus
    {
        public string? SyllabusName { get; set; }
        public string? EffectiveDate { get; set; }
        public string? StudentTasks { get; set; }
        public double ScoringScale { get; set; }
        public int TimePerSession { get; set; }
        public double MinAvgMarkToPass { get; set; }
        public string? Description { get; set; }
        public string? SubjectCode { get; set; }
        public string? SyllabusLink { get; set; }
        public string? Type {  get; set; }
    }
}
