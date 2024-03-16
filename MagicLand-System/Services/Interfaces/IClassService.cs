using MagicLand_System.Domain.Models;
using MagicLand_System.Enums;
using MagicLand_System.PayLoad.Request.Class;
using MagicLand_System.PayLoad.Request.Student;
using MagicLand_System.PayLoad.Response.Attendances;
using MagicLand_System.PayLoad.Response.Class;
using MagicLand_System.PayLoad.Response.Classes;
using MagicLand_System.PayLoad.Response.Classes.ForLecturer;
using MagicLand_System.PayLoad.Response.Schedules;
using MagicLand_System.PayLoad.Response.Schedules.ForLecturer;
using MagicLand_System.PayLoad.Response.Students;
using MagicLand_System.PayLoad.Response.Topics;

namespace MagicLand_System.Services.Interfaces
{
    public interface IClassService
    {
        Task<List<ClassWithSlotShorten>> GetClassesNotInCartAsync(Guid? courseId);
        Task<List<ClassWithSlotShorten>> GetClassesAsync(PeriodTimeEnum time);
        Task<List<ClassWithSlotShorten>> GetClassesByCourseIdAsync(Guid id, ClassStatusEnum status);
        Task<TopicResponse> GetTopicLearningAsync(Guid classId, int topicOrder);
        Task<List<ClassWithSlotShorten>> GetValidClassForStudentAsync(Guid courseId, Guid studentId);
        Task<List<StudentResponse>> GetValidStudentForClassAsync(Guid classId, List<Student> students);
        Task<List<ClassWithSlotShorten>> FilterClassAsync(List<string>? keyWords, int? leastNumberStudent, int? limitStudent, PeriodTimeEnum time);
        Task<ClassResExtraInfor> GetClassByIdAsync(Guid id);
        Task<bool> CreateNewClass(CreateClassRequest request);
        Task<List<MyClassResponse>> GetAllClass(string searchString = null, string status = null);
        Task<MyClassResponse> GetClassDetail(string id);
        Task<List<StudentInClass>> GetAllStudentInClass(string id);
        Task<string> AutoCreateClassCode(string courseId);
        Task<bool> UpdateClass(string classId, UpdateClassRequest request);
        Task<List<ClassProgressResponse>> GetClassProgressResponsesAsync(string classId);
        Task ValidateScheduleAmongClassesAsync(List<Guid> classIdList);
        Task<List<ClassForAttendance>> GetAllClassForAttandance(string? searchString, DateTime dateTime, string? attendanceStatus);
        Task<List<ClassWithSlotOutSideResponse>> GetCurrentLectureClassesScheduleAsync();
        Task<ScheduleWithAttendanceResponse> GetAttendanceOfClassesInDateAsync(Guid classId, DateTime date);
        Task<List<ClassWithDailyScheduleRes>> GetSuitableClassAsync(Guid classId, List<Guid> studentIdList);
        Task<string> ChangeStudentClassAsync(Guid fromClassId, Guid toClassId, List<Guid> studentIdList);
        Task<bool> CancelClass(string classId);
        Task<bool> UpdateSession(string sessionId, UpdateSessionRequest request);
        Task<bool> MakeUpClass(string StudentId, string ScheduleId, MakeupClassRequest request);
        Task<List<ScheduleResponse>> GetScheduleCanMakeUp(string scheduleId, string studentId, DateTime? date = null, string? keyword = null, string? slotId = null);
        Task<InsertClassesResponse> InsertClasses(List<CreateClassesRequest> request);
        Task<ClassFromClassCode> GetClassFromClassCode(string classCode);
    }
}
