using MagicLand_System.Domain.Models;
using MagicLand_System.PayLoad.Request.Class;
using MagicLand_System.PayLoad.Response.Classes;

namespace MagicLand_System.Services.Interfaces
{
    public interface IClassService
    {
        Task<List<ClassResExtraInfor>> GetClassesAsync();
        Task<List<ClassResExtraInfor>> GetClassesByCourseIdAsync(Guid id);
        Task<List<ClassResExtraInfor>> FilterClassAsync(List<string>? keyWords, int? leastNumberStudent, int? limitStudent);
        Task<ClassResExtraInfor> GetClassByIdAsync(Guid id);
        Task<bool> CreateNewClass(CreateClassRequest request);
        Task<List<MyClassResponse>> GetAllClass(string classCode = null);
    }
}
