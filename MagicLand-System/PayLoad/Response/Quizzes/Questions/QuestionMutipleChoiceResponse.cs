using MagicLand_System.PayLoad.Response.Quizzes.Answers;

namespace MagicLand_System.PayLoad.Response.Quizzes.Questions
{
    public class QuestionMutipleChoiceResponse : QuizResponse
    {
        public List<MutilpleChoiceAnswerResponse> Answers { get; set; } = new List<MutilpleChoiceAnswerResponse>();

    }
}
