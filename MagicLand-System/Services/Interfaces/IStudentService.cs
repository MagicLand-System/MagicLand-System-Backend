using MagicLand_System.Domain.Models;
using MagicLand_System.Enums;
using MagicLand_System.PayLoad.Request.Attendance;
using MagicLand_System.PayLoad.Request.Student;
using MagicLand_System.PayLoad.Response.Attendances;
using MagicLand_System.PayLoad.Response.Classes;
using MagicLand_System.PayLoad.Response.Courses;
using MagicLand_System.PayLoad.Response.Students;

namespace MagicLand_System.Services.Interfaces
{
    public interface IStudentService
    {
        Task<bool> AddStudent(CreateStudentRequest request);
        Task<List<ClassResExtraInfor>> GetClassOfStudent(String studentId,string status);
        Task<List<StudentScheduleResponse>> GetScheduleOfStudent(string studentId);
        Task<List<Student>> GetStudentsOfCurrentParent();
        Task<StudentResponse> UpdateStudentAsync(UpdateStudentRequest newStudentInfor, Student oldStudentInfor);
        Task<string> DeleteStudentAsync(Student student);
        Task<string> TakeStudentAttendanceAsync(AttendanceRequest request);
        Task<List<AttendanceResponse>> GetStudentAttendanceFromClassInNow(Guid classId);
        Task<StudentResponse> GetStudentById(Guid id);
        Task<List<StudentStatisticResponse>> GetStatisticNewStudentRegisterAsync(PeriodTimeEnum time);
    }
}
