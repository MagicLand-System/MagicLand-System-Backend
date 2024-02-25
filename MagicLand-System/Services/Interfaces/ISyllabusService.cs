using MagicLand_System.PayLoad.Request.Course;
using MagicLand_System.PayLoad.Response.Quizes;
using MagicLand_System.PayLoad.Response.Quizzes;
using MagicLand_System.PayLoad.Response.Syllabuses;
using static Microsoft.Extensions.Logging.EventSource.LoggingEventSource;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace MagicLand_System.Services.Interfaces
{
    public interface ISyllabusService
    {
        Task<bool> AddSyllabus(OverallSyllabusRequest request);
        Task<(SyllabusResponse?, SyllabusWithScheduleResponse?)> LoadSyllabusByCourseIdAsync(Guid courseId, Guid classId);
        Task<(SyllabusResponse?, SyllabusWithCourseResponse?)> LoadSyllabusByIdAsync(Guid id);
        Task<List<SyllabusWithCourseResponse>> LoadSyllabusesAsync();
        Task<List<SyllabusWithCourseResponse>> FilterSyllabusAsync(List<string>? keyWords,DateTime? date,double? score);
        Task<List<QuizResponse>> LoadQuizzesAsync();
        Task<List<QuizResponse>> LoadQuizzesByCourseIdAsync(Guid id);
        Task<List<QuizResponse>> LoadQuizzesByClassIdAsync(Guid id);
        Task<List<SyllabusResponseV2>> GetAllSyllabus(string? keyword);
        Task<bool> UpdateSyllabus(OverallSyllabusRequest request, string id);

    }
}
