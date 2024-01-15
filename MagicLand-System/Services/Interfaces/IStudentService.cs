using MagicLand_System.Domain.Models;
using MagicLand_System.PayLoad.Request.Student;
using MagicLand_System.PayLoad.Response.Classes;
using MagicLand_System.PayLoad.Response.Students;

namespace MagicLand_System.Services.Interfaces
{
    public interface IStudentService
    {
        Task<bool> AddStudent(CreateStudentRequest request);
        Task<List<ClassResExtraInfor>> GetClassOfStudent(String studentId,string status);
        Task<List<StudentScheduleResponse>> GetScheduleOfStudent(string studentId);
        Task<List<Student>> GetStudentsOfCurrentParent();
    }
}
