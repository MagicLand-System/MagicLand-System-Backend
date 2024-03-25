using MagicLand_System.Enums;
using MagicLand_System.PayLoad.Request.Course;
using MagicLand_System.PayLoad.Request.Syllabus;
using MagicLand_System.PayLoad.Response.Quizes;
using MagicLand_System.PayLoad.Response.Quizzes;
using MagicLand_System.PayLoad.Response.Quizzes.Questions;
using MagicLand_System.PayLoad.Response.Quizzes.Staff;
using MagicLand_System.PayLoad.Response.Sessions;
using MagicLand_System.PayLoad.Response.Syllabuses;
using MagicLand_System.PayLoad.Response.Syllabuses.ForStaff;

namespace MagicLand_System.Services.Interfaces
{
    public interface ISyllabusService
    {
        Task<bool> AddSyllabus(OverallSyllabusRequest request);
        Task<string> CheckingSyllabusInfor(string name, string code);
        Task<SyllabusResponse> LoadSyllabusDynamicIdAsync(Guid courseId, Guid classId);
        Task<SyllabusResponse> LoadSyllabusByIdAsync(Guid id);
        Task<List<SyllabusResponse>> LoadSyllabusesAsync();
        Task<List<SyllabusResponse>> FilterSyllabusAsync(List<string>? keyWords, DateTime? date, double? score);
        Task<List<ExamWithQuizResponse>> LoadQuizzesAsync();
        Task<List<ExamWithQuizResponse>> LoadQuizzesByCourseIdAsync(Guid id);
        Task<List<ExamResForStudent>> LoadExamOfClassByClassIdAsync(Guid id, Guid? studentId);
        Task<List<ExamExtraInfor>> LoadExamOfCurrentStudentAsync(int numberOfDate);
        Task<List<QuizResponse>> LoadQuizOfExamByExamIdAsync(Guid id, int? examPart);
        Task<List<SyllabusResponseV2>> GetAllSyllabus(string? keyword);
        Task<bool> UpdateSyllabus(OverallSyllabusRequest request, string id);
        Task<StaffSyllabusResponse> GetStaffSyllabusResponse(string id);
        Task<List<StaffQuestionResponse>> GetStaffQuestions(string questionpackageId);
        Task<List<SyllabusResponseV2>> GetStaffSyllabusCanInsert(string? keyword);
        Task<bool> UpdateQuiz(string questionpackageId, UpdateQuestionPackageRequest request);
        Task<bool> UpdateOverallSyllabus(string id, UpdateOverallSyllabus updateOverallSyllabus);
        Task<bool> UpdateTopic(string id, UpdateTopicRequest request);
        Task<bool> UpdateSession(string id, UpdateSessionRequest request);
        Task<List<StaffMaterialResponse>> GetMaterialResponse(string syllabusId);
        Task<List<StaffExamSyllabusResponse>> GetStaffExamSyllabusResponses(string syllabusId);
        Task<List<StaffSessionResponse>> GetAllSessionResponses(string syllabusId);
        Task<List<StaffQuestionPackageResponse>> GetStaffQuestionPackageResponses(string sylId);
        Task<GeneralSyllabusResponse> GetGeneralSyllabusResponse(string syllabusId);
    }
}
