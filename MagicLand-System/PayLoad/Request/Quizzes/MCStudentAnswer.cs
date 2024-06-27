namespace MagicLand_System.PayLoad.Request.Quizzes
{
    public class MCStudentAnswer
    {
        public required Guid QuestionId { get; set; }
        public required Guid AnswerId { get; set; }
    }
}
