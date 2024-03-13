namespace MagicLand_System.PayLoad.Request.Quizzes
{
    public class QuizRequest
    {
        public required Guid ClassId { get; set; }
        public required Guid ExamId { get; set; }
    }
}
