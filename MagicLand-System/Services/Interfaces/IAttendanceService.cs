using MagicLand_System.PayLoad.Request.Attendance;
using MagicLand_System.PayLoad.Response.Attendances;
using MagicLand_System.PayLoad.Response.Schedules;

namespace MagicLand_System.Services.Interfaces
{
    public interface IAttendanceService
    {
        Task<List<StaffAttandaceResponse>> LoadAttandance(string scheduleId, string? searchString);
        Task<bool> TakeAttandance(List<StaffClassAttandanceRequest> requests);
        Task<AttendanceWithClassResponse> GetAttendanceOfClassAsync(Guid id);
        Task<List<AttendanceWithClassResponse>> GetAttendanceOfClassStudent(Guid id);
        Task<List<AttendanceWithClassResponse>> GetAttendanceOfClassesAsync();
        Task<List<AttendanceWithClassResponse>> GetAttendanceOfClassesOfCurrentUserAsync();
    }
}
