using MagicLand_System.PayLoad.Request.Course;
using MagicLand_System.PayLoad.Response.Syllabuses;
using static Microsoft.Extensions.Logging.EventSource.LoggingEventSource;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace MagicLand_System.Services.Interfaces
{
    public interface ISyllabusService
    {
        Task<bool> AddSyllabus(OverallSyllabusRequest request);
        Task<SyllabusResponse> LoadSyllabusByCourseIdAsync(Guid id);
        Task<SyllabusResponse> LoadSyllabusByIdAsync(Guid id);
        Task<List<SyllabusResponse>> LoadSyllabusesAsync();
        Task<List<SyllabusResponse>> FilterSyllabusAsync(List<string>? keyWords,DateTime? date,double? score);
        Task<List<SyllabusResponseV2>> GetAllSyllabus(string? keyword);
        Task<bool> UpdateSyllabus(OverallSyllabusRequest request, string id);

    }
}
