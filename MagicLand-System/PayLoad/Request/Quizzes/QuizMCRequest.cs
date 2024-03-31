namespace MagicLand_System.PayLoad.Request.Quizzes
{
    public class QuizMCRequest : QuizRequest
    {
        public required List<MCStudentAnswer> StudentQuestionResults { get; set; }
    }
}
