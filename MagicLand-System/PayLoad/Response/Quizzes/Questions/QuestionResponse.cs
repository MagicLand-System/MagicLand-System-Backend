using MagicLand_System.PayLoad.Response.Quizzes.Answers;

namespace MagicLand_System.PayLoad.Response.Quizzes.Questions
{
    public class QuestionResponse
    {
        public Guid QuestionId { get; set; }
        public string? QuestionDescription { get; set; } = string.Empty;
        public string? QuestionImage { get; set; } = string.Empty;
        public List<FlashCardAnswerResponse>? AnwserFlashCarsInfor { get; set; }
        public List<MutilpleChoiceAnswerResponse>? AnswersMutipleChoicesInfor { get; set; }

    }
}
