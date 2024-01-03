using MagicLand_System.Domain.Models;
using MagicLand_System.PayLoad.Request.Class;
using MagicLand_System.PayLoad.Response.Class;

namespace MagicLand_System.Services.Interfaces
{
    public interface IClassService
    {
        Task<List<ClassResponse>> GetClassesAsync();
        Task<List<ClassResponse>> GetClassesByCourseIdAsync(Guid id);
        Task<List<ClassResponse>> FilterClassAsync(List<string>? keyWords, int? leastNumberStudent, int? limitStudent);
        Task<ClassResponse> GetClassByIdAsync(Guid id);
        Task<bool> CreateNewClass(CreateClassRequest request);
        Task<List<MyClassResponse>> GetAllClass(string classCode = null);
    }
}
