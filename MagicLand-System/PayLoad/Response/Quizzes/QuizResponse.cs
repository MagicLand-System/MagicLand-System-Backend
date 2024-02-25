using MagicLand_System.PayLoad.Response.Quizzes.Questions;

namespace MagicLand_System.PayLoad.Response.Quizes
{
    public class QuizResponse
    {
        public string? QuizCategory { get; set; } = string.Empty;
        public string? QuizType { get; set; } = string.Empty;
        public string? QuizName { get; set; }
        public double Weight { get; set; }
        public double CompleteionCriteria { get; set; }
        public double TotalMark { get; set; }
        public int TotalQuestion { get; set; }
        public string? Duration { get; set; } = string.Empty;
        public string? Date { get; set; } = string.Empty;
        public int Attempt { get; set; } = 1;
        public int NoSession { get; set; }
        public Guid SessionId { get; set; }
        public Guid CourseId { get; set; } 
        public Guid ExamId { get; set ; }
        public List<QuestionResponse> Questions { get; set; } = new List<QuestionResponse>();

    }
}
