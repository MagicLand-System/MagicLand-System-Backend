using MagicLand_System.Domain.Models;
using MagicLand_System.PayLoad.Request;
using MagicLand_System.PayLoad.Response;

namespace MagicLand_System.Services.Interfaces
{
    public interface ICourseService
    {
        Task<List<CourseResponse>> GetCoursesAsync();
        Task<CourseResponse> GetCourseByIdAsync(Guid id);
        Task<List<CourseResponse>> SearchCourseAsync(string keyWord);
        Task<List<CourseResponse>> FilterCourseAsync(string? keyWord, int? minYearsOld, int? maxYearsOld, int? numberOfSession);
    }
}
