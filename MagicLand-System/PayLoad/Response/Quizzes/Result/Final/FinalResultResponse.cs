namespace MagicLand_System.PayLoad.Response.Quizzes.Result.Final
{
    public class FinalResultResponse
    {
        public Guid ClassId { get; set; }
        public Guid CourseId { get; set; }
        public Guid StudentId { get; set; }
        public string? ClassName { get; set; }
        public string? CourseName { get; set; }
        public string? StudentName { get; set; }
        public List<FinalExamResultResponse>? QuizzesResults { get; set; }
        public Participation? ParticipationResult { get; set; }
        public double Average { get; set; }
        public bool IsRate { get; set; }
        public string? Status { get; set; }
    }
}
