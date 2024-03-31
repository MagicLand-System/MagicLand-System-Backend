using MagicLand_System.PayLoad.Response.Quizzes;

namespace MagicLand_System.PayLoad.Response.Customs
{
    public class QuizApiCustomResponse
    {
        public List<QuizMultipleChoiceResponse>? QuizMutilpleChoice { get; set; }
        public List<QuizFlashCardResponse>? QuizFlashCard { get; set; }
    }
}
