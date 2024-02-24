using MagicLand_System.PayLoad.Response.Quizes;

namespace MagicLand_System.PayLoad.Response.Quizzes
{
    public class QuizMultipleChoiceResponse : QuizResponse
    {
       public List<QuestionMutipleChoiceResponse>? QuestionMultipleChoices { get; set; } = new List<QuestionMutipleChoiceResponse>();
    }
}
