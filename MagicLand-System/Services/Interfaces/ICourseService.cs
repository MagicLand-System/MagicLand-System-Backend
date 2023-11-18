using MagicLand_System.Domain.Models;
using MagicLand_System.PayLoad.Request;

namespace MagicLand_System.Services.Interfaces
{
    public interface ICourseService
    {
        Task<List<Course>> GetCoursesAsync();
        Task<List<Course>> GetCourseByIdAsync(Guid id);
        Task<List<Course>> SearchCourseAsync(string keyWord);
        Task<List<Course>> FilterCourseAsync(int minYearsOld , string? keyWord = null, int? maxYearsOld = null,int? numberOfSession = null);
    }
}
