using MagicLand_System.PayLoad.Response.Quizzes.Answers;

namespace MagicLand_System.PayLoad.Response.Quizzes.Questions
{
    public class QuestionMutipleChoiceResponse : QuestionResponse
    {
        public List<MutilpleChoiceAnswerResponse> Answers { get; set; } = new List<MutilpleChoiceAnswerResponse>();

    }
}
