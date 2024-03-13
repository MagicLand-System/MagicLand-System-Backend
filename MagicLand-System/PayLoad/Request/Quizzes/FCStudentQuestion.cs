namespace MagicLand_System.PayLoad.Request.Quizzes
{
    public class FCStudentQuestion
    {
        public required Guid QuestionId { get; set; }
        public required List<FCStudentAnswer> Answers { get; set; }
    }
}
