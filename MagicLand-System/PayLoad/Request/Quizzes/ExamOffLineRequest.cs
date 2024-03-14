namespace MagicLand_System.PayLoad.Request.Quizzes
{
    public class ExamOffLineRequest : QuizRequest
    {
        public required List<StudentExamGrade> StudentQuizGardes { get; set; }
    }
}
