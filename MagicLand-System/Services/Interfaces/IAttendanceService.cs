using MagicLand_System.PayLoad.Response.Attendances;

namespace MagicLand_System.Services.Interfaces
{
    public interface IAttendanceService
    {
        Task<AttendanceWithClassResponse> GetAttendanceOfClassAsync(Guid id);
        Task<List<AttendanceWithClassResponse>> GetAttendanceOfClassStudent(Guid id);
        Task<List<AttendanceWithClassResponse>> GetAttendanceOfClassesAsync();
        Task<List<AttendanceWithClassResponse>> GetAttendanceOfClassesOfCurrentUserAsync();
    }
}
