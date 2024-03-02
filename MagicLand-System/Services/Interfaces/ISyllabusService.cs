using MagicLand_System.PayLoad.Request.Course;
using MagicLand_System.PayLoad.Request.Syllabus;
using MagicLand_System.PayLoad.Response.Quizes;
using MagicLand_System.PayLoad.Response.Quizzes;
using MagicLand_System.PayLoad.Response.Quizzes.Questions;
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
        Task<List<ExamWithQuizResponse>> LoadQuizzesAsync();
        Task<List<ExamWithQuizResponse>> LoadQuizzesByCourseIdAsync(Guid id);
        Task<List<ExamResponse>> LoadExamOfClassByClassIdAsync(Guid id);
        Task<List<QuizResponse>> LoadQuizOfExamByExamIdAsync(Guid id, int examPart);
        Task<List<SyllabusResponseV2>> GetAllSyllabus(string? keyword);
        Task<bool> UpdateSyllabus(OverallSyllabusRequest request, string id);
        Task<StaffSyllabusResponse> GetStaffSyllabusResponse(string id);
        Task<List<StaffQuestionResponse>> GetStaffQuestions(string questionpackageId);
        Task<List<SyllabusResponseV2>> GetStaffSyllabusCanInsert(string? keyword);
        Task<bool> UpdateQuiz(string questionpackageId, UpdateQuestionPackageRequest request);
        Task<bool> UpdateOverallSyllabus(string id,UpdateOverallSyllabus updateOverallSyllabus);
        Task<bool> UpdateTopic(string id,UpdateTopicRequest request);
        Task<bool> UpdateSession(string id,UpdateSessionRequest request);
    }
}
