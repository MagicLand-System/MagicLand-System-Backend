using MagicLand_System.PayLoad.Response.Quizes;
using MagicLand_System.PayLoad.Response.Quizzes.Questions;

namespace MagicLand_System.PayLoad.Response.Quizzes
{
    public class QuizMultipleChoiceResponse : ExamResponse
    {
       public List<QuestionMCResponse>? QuestionMultipleChoices { get; set; } = new List<QuestionMCResponse>();
    }
}
