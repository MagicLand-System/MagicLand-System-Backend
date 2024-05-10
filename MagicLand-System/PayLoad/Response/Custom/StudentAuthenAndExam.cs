using MagicLand_System.PayLoad.Response.Quizes;
using MagicLand_System.PayLoad.Response.Quizzes;

namespace MagicLand_System.PayLoad.Response.Custom
{
    public class StudentAuthenAndExam
    {
        public List<LoginResponse> StudentAuthen { get; set; } = new List<LoginResponse>();
        public List<ExamResForStudent> Exams { get; set; } = new List<ExamResForStudent>();
    }
}
