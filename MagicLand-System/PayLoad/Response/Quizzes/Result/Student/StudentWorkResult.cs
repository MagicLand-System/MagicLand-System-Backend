namespace MagicLand_System.PayLoad.Response.Quizzes.Result.Student
{
    public class StudentWorkResult
    {
        public Guid QuestionId { get; set; }
        public string? QuestionDescription { get; set; } = string.Empty;
        public string? QuestionImage { get; set; } = string.Empty;
        public MCAnswerResultResponse? MultipleChoiceAnswerResult { get; set; }
    }
}
