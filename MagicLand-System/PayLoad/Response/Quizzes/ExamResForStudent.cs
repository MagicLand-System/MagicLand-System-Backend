using MagicLand_System.PayLoad.Response.Quizes;

namespace MagicLand_System.PayLoad.Response.Quizzes
{
    public class ExamResForStudent : ExamResponse
    {
        public double? Score { get; set; }
        public int? AttemptLeft { get; set; }
    }
}
