namespace MagicLand_System.PayLoad.Response.Quizzes.Result.Student
{
    public class QuizResultWithStudentWork
    {
        public required Guid StudentId { get; set; }
        public required string StudentName { get; set; } = string.Empty;
        public List<StudentWorkFullyInfor>? ExamInfors { get; set; }

    }
}
