namespace MagicLand_System.PayLoad.Response.Quizzes
{
    public class QuizInforResponse
    {
        public Guid ExamId { get; set; }
        public required string ExamName { get; set; }
        public required int ExamPart { get; set; }
        public required string QuizName { get; set; }
        public int? QuizDuration { get; set; }
        public int? Attempts { get; set; }
        public DateTime? QuizStartTime { get; set; }
        public DateTime? QuizEndTime { get; set; }

    }
}
