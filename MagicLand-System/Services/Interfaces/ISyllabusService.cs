using MagicLand_System.PayLoad.Request.Course;
using MagicLand_System.PayLoad.Response.Syllabuses;

namespace MagicLand_System.Services.Interfaces
{
    public interface ISyllabusService
    {
        Task<bool> AddSyllabus(OverallSyllabusRequest request);
        Task<SyllabusResponse> LoadSyllabusByCourseIdAsync(Guid id);
        Task<SyllabusResponse> LoadSyllabusByIdAsync(Guid id);
    }
}
