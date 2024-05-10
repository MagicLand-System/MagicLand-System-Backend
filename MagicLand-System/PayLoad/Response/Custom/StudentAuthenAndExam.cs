using MagicLand_System.PayLoad.Response.Quizes;

namespace MagicLand_System.PayLoad.Response.Custom
{
    public class StudentAuthenAndExam
    {
        public List<LoginResponse> StudentAuthen { get; set; } = new List<LoginResponse>();
        public List<ExamResponse> Exams { get; set; } = new List<ExamResponse>();
    }
}
