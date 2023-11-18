using MagicLand_System.Domain.Models;
using MagicLand_System.PayLoad.Response;

namespace MagicLand_System.Services.Interfaces
{
    public interface IClassService
    {
        Task<List<ClassResponse>> GetClassesAsync();
        Task<List<ClassResponse>> GetClassesByCourseIdAsync(Guid id);
        Task<List<ClassResponse>> SearchClass();
        Task<List<ClassResponse>> FilterClass();
        Task<ClassResponse> GetClassById(Guid id);
    }
}
