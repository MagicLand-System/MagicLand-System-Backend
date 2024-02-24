namespace MagicLand_System.PayLoad.Response.Quizes
{
    public class QuizResponse
    {
        public string? QuizType { get; set; } = string.Empty;
        public double Weight { get; set; }
        public double CompleteionCriteria { get; set; }
        public double TotalMark { get; set; }
        public int TotalQuestion { get; set; }
        public string? Duration { get; set; } = string.Empty;
        public string? Date { get; set; } = string.Empty;
        public int Attempt { get; set; } = 1;
        public int NoSession { get; set; }
        public string? QuestionTitle { get; set; }
        public string? QuestionType { get; set; }
        public Guid SessionId { get; set; }
        public Guid CourseId { get; set; } 

    }
}
