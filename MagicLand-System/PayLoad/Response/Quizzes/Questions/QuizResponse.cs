using MagicLand_System.PayLoad.Response.Quizzes.Answers;

namespace MagicLand_System.PayLoad.Response.Quizzes.Questions
{
    public class QuizResponse
    {
        public Guid QuestionId { get; set; }
        public string? QuestionDescription { get; set; } = string.Empty;
        public string? QuestionImage { get; set; } = string.Empty;
        public List<FCAnswerResponse>? AnwserFlashCarsInfor { get; set; }
        public List<MCAnswerResponse>? AnswersMutipleChoicesInfor { get; set; }

    }
}
