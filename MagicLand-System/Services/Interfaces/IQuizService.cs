using MagicLand_System.PayLoad.Request.Quizzes;
using MagicLand_System.PayLoad.Response.Quizzes.Result;
using MagicLand_System.PayLoad.Response.Quizzes.Result.Final;

namespace MagicLand_System.Services.Interfaces
{
    public interface IQuizService
    {
        Task<List<FinalResultResponse>> GetFinalResultAsync(List<Guid> studentIdList);
        Task<List<QuizResultExtraInforResponse>> GetCurrentStudentQuizDoneAsync();
        Task<QuizResultResponse> GradeQuizMCAsync(QuizMCRequest quizStudentWork);
        Task<QuizResultResponse> GradeQuizFCAsync(Guid classId, Guid examId, double score);
        Task<string> GradeExamOffLineAsync(ExamOffLineRequest exaOffLineStudentWork);
        Task<string> SettingExamTimeAsync(Guid examId, Guid classId, SettingQuizTimeRequest settingInfor);
    }
}
