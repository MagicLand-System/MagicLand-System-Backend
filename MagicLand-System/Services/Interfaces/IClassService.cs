using MagicLand_System.Domain.Models;
using MagicLand_System.Enums;
using MagicLand_System.PayLoad.Request.Class;
using MagicLand_System.PayLoad.Request.Student;
using MagicLand_System.PayLoad.Response.Attendances;
using MagicLand_System.PayLoad.Response.Class;
using MagicLand_System.PayLoad.Response.Classes;
using MagicLand_System.PayLoad.Response.Schedules;

namespace MagicLand_System.Services.Interfaces
{
    public interface IClassService
    {
        Task<List<ClassResExtraInfor>> GetClassesAsync(PeriodTimeEnum time);
        Task<List<ClassResExtraInfor>> GetClassesByCourseIdAsync(Guid id);
        Task<List<ClassResExtraInfor>> FilterClassAsync(List<string>? keyWords, int? leastNumberStudent, int? limitStudent, PeriodTimeEnum time);
        Task<ClassResExtraInfor> GetClassByIdAsync(Guid id);
        Task<bool> CreateNewClass(CreateClassRequest request);
        Task<List<MyClassResponse>> GetAllClass(string searchString = null,string status = null);
        Task<MyClassResponse> GetClassDetail(string id);
        Task<List<StudentInClass>> GetAllStudentInClass(string id);
        Task<string> AutoCreateClassCode(string courseId);
        Task<bool> UpdateClass(string classId, UpdateClassRequest request);
        Task<List<ClassProgressResponse>> GetClassProgressResponsesAsync(string classId);
        Task ValidateScheduleAmongClassesAsync(List<Guid> classIdList);
        Task<List<ClassForAttendance>> GetAllClassForAttandance(string? searchString, DateTime dateTime,string? attendanceStatus);
        Task<List<ClassResponse>> GetCurrentLectureClassesAsync();
        Task<List<AttendanceWithClassResponse>> GetAttendanceOfClassesInDateAsync(List<Guid> classIdList, DateTime date);
        Task<List<ClassResponse>> GetSuitableClassAsync(Guid classId, List<Guid> studentIdList);
        Task<string> ChangeStudentClassAsync(Guid fromClassId, Guid toClassId, List<Guid> studentIdList);
        Task<bool> CancelClass(string classId); 
    }
}
