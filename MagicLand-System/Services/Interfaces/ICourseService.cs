using MagicLand_System.Domain.Models;
using MagicLand_System.PayLoad.Response.Course;

namespace MagicLand_System.Services.Interfaces
{
    public interface ICourseService
    {
        Task<List<CourseResponse>> GetCoursesAsync();
        Task<CourseResponse> GetCourseByIdAsync(Guid id);
        Task<List<CourseResponse>> SearchCourseByNameAsync(string keyWord);
        Task<List<CourseResponse>> FilterCourseAsync(int minYearsOld, int maxYearsOld, int? minNumberSession,
            int? maxNumberSession, double minPrice, double? maxPrice, string? subject, int? rate);
        Task<List<CourseCategory>> GetCourseCategories();
    }
}
