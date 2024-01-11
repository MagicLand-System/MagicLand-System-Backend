using MagicLand_System.Domain.Models;
using MagicLand_System.PayLoad.Request.Class;
using MagicLand_System.PayLoad.Response.Class;

namespace MagicLand_System.Services.Interfaces
{
    public interface IClassService
    {
        Task<List<ClassResponseV1>> GetClassesAsync();
        Task<List<ClassResponseV1>> GetClassesByCourseIdAsync(Guid id);
        Task<List<ClassResponseV1>> FilterClassAsync(List<string>? keyWords, int? leastNumberStudent, int? limitStudent);
        Task<ClassResponseV1> GetClassByIdAsync(Guid id);
        Task<bool> CreateNewClass(CreateClassRequest request);
        Task<List<ClassResponseV2>> GetAllClass(string searchString = null , string status  = null);
        Task<ClassResponseV2> GetClassDetail(string id);
        Task<List<StudentInClass>> GetAllStudentInClass(string id);
    }
}
