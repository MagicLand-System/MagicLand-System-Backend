using MagicLand_System.Domain.Models;

namespace MagicLand_System.Services.Interfaces
{
    public interface IClassService
    {
        Task<List<Class>> GetClassesAsync();
        Task<List<Class>> GetClassesByCourseIdAsync(Guid id);
        Task<List<Class>> SearchClass();
        Task<List<Class>> FilterClass();
        Task<Class> GetClassById(Guid id);
    }
}
