using MagicLand_System.PayLoad.Response.Quizes;
using MagicLand_System.PayLoad.Response.Quizzes.Questions;

namespace MagicLand_System.PayLoad.Response.Quizzes
{
    public class QuizFlashCardResponse : QuizResponse
    {
        public List<QuestionFlashCardResponse>? QuestionFlasCards { get; set; } = new List<QuestionFlashCardResponse>();
    }
}
