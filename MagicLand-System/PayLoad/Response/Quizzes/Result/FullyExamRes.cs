using MagicLand_System.PayLoad.Response.Quizes;

namespace MagicLand_System.PayLoad.Response.Quizzes.Result
{
    public class FullyExamRes : ExamResponse
    {
        public List<StudentWorkResult>? StudentWork { get; set; }
    }
}
