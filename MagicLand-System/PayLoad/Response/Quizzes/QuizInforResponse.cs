namespace MagicLand_System.PayLoad.Response.Quizzes
{
    public class QuizInforResponse
    {
        public Guid ExamId { get; set; }
        public required string ExamName { get; set; }
        public required int ExamPart { get; set; }
        public required string QuizName { get; set; }
        //public required int QuizDuration { get; set; }
        //public required int Attempts { get; set; }
        //public required DateTime QuizStartTime { get; set; }
        //public required DateTime QuizEndTime { get; set; }

    }
}
