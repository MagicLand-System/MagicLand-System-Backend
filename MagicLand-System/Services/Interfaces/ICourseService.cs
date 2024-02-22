using MagicLand_System.Domain.Models;
using MagicLand_System.PayLoad.Response.Courses;

namespace MagicLand_System.Services.Interfaces
{
    public interface ICourseService
    {
        Task<List<CourseResExtraInfor>> GetCoursesAsync();
        Task<CourseResExtraInfor> GetCourseByIdAsync(Guid id);
        Task<List<CourseResExtraInfor>> SearchCourseByNameOrAddedDateAsync(string keyWord);
        Task<List<CourseResExtraInfor>> FilterCourseAsync(int minYearsOld, int maxYearsOld, int? minNumberSession,
            int? maxNumberSession, double minPrice, double? maxPrice, string? subject, int? rate);
        Task<List<CourseCategory>> GetCourseCategories();
        Task<List<CourseResExtraInfor>> GetCoursesOfStudentByIdAsync(Guid studentId);
    }
}
