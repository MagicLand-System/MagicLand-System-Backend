using MagicLand_System.PayLoad.Request.Evaluates;
using MagicLand_System.PayLoad.Response.Courses;
using MagicLand_System.PayLoad.Response.Custom;
using MagicLand_System.PayLoad.Response.Users;

namespace MagicLand_System.Services.Interfaces
{
    public interface IDeveloperService
    {
        Task<List<StudentLearningInfor>> TakeFullAttendanceAndEvaluateAsync(Guid classId, int percentageAbsent, List<EvaluateDataRequest> noteEvaluate);
        Task<List<AccountResponse>> GetStudentAccount(List<Guid> studentIdList);
    }
}
