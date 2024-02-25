using MagicLand_System.PayLoad.Response.Quizzes.Answers;

namespace MagicLand_System.PayLoad.Response.Quizzes.Questions
{
    public class QuestionFlashCardResponse : QuestionResponse
    {
        public List<FlashCardAnswerResponse> FlashCars { get; set; } = new List<FlashCardAnswerResponse>();
    }
}
