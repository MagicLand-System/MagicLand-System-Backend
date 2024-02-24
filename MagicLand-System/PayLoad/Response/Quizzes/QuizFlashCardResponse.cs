using MagicLand_System.PayLoad.Response.Quizes;

namespace MagicLand_System.PayLoad.Response.Quizzes
{
    public class QuizFlashCardResponse : QuizResponse
    {
        public List<QuestionFlashCardResponse>? QuestionFlasCards { get; set; } = new List<QuestionFlashCardResponse>();
    }
}
