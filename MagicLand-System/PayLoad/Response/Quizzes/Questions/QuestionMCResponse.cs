using MagicLand_System.PayLoad.Response.Quizzes.Answers;

namespace MagicLand_System.PayLoad.Response.Quizzes.Questions
{
    public class QuestionMCResponse : QuizResponse
    {
        public List<MCAnswerResponse> Answers { get; set; } = new List<MCAnswerResponse>();

    }
}
