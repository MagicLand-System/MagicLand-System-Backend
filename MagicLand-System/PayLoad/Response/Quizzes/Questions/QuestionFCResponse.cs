using MagicLand_System.PayLoad.Response.Quizzes.Answers;

namespace MagicLand_System.PayLoad.Response.Quizzes.Questions
{
    public class QuestionFCResponse : QuizResponse
    {
        public List<FlashCardAnswerResponseDefault> FlashCars { get; set; } = new List<FlashCardAnswerResponseDefault>();
    }
}
