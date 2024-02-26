using MagicLand_System.PayLoad.Response.Quizes;
using MagicLand_System.PayLoad.Response.Quizzes.Questions;

namespace MagicLand_System.PayLoad.Response.Quizzes
{
    public class ExamWithQuizResponse : ExamResponse
    {
        public List<QuizResponse> Quizzes { get; set; } = new List<QuizResponse>();
    }
}
