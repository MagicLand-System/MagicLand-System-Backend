using MagicLand_System.PayLoad.Request.Evaluates;
using MagicLand_System.PayLoad.Response.Courses;
using MagicLand_System.PayLoad.Response.Custom;

namespace MagicLand_System.Services.Interfaces
{
    public interface IDeveloperService
    {
        Task<List<StudentLearningInfor>> TakeFullAttendanceAndEvaluateAsync(Guid classId, int percentageAbsent, List<EvaluateDataRequest> noteEvaluate);
    }
}
