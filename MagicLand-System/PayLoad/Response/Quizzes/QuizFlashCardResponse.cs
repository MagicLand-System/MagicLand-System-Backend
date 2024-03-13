using MagicLand_System.PayLoad.Response.Quizes;
using MagicLand_System.PayLoad.Response.Quizzes.Questions;

namespace MagicLand_System.PayLoad.Response.Quizzes
{
    public class QuizFlashCardResponse : ExamResponse
    {
        public List<QuestionFCResponse>? QuestionFlasCards { get; set; } = new List<QuestionFCResponse>();
    }
}
