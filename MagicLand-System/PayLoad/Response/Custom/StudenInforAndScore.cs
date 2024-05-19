using MagicLand_System.PayLoad.Response.Attendances;
using MagicLand_System.PayLoad.Response.Quizzes.Result.Final;
using MagicLand_System.PayLoad.Response.Quizzes.Result.Student;
using MagicLand_System.PayLoad.Response.Students;
using MagicLand_System.PayLoad.Response.Users;

namespace MagicLand_System.PayLoad.Response.Custom
{
    public class StudenInforAndScore 
    {
        public StudentResponse? StudentInfor { get; set; }
        public UserResponse? ParentInfor { get; set; }
        public List<StudentWorkFullyInfor>? ExamInfors { get; set; }
        public Participation? ParticipationInfor { get; set; }
    }
}
