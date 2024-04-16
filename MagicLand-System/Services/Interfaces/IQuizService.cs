using MagicLand_System.PayLoad.Request.Quizzes;
using MagicLand_System.PayLoad.Response.Quizzes.Questions;
using MagicLand_System.PayLoad.Response.Quizzes.Result;
using MagicLand_System.PayLoad.Response.Quizzes.Result.Final;
using MagicLand_System.PayLoad.Response.Quizzes.Result.Student;

namespace MagicLand_System.Services.Interfaces
{
    public interface IQuizService
    {
        //Task<FullyExamRes> GetFullyExamInforStudent(Guid studentId, Guid examId);
        //Task<List<FCQuizResponse>> GetFCQuestionPackageAsync(Guid examId);
        Task<List<FinalResultResponse>> GetFinalResultAsync(List<Guid> studentIdList);
        Task<List<QuizResultExtraInforResponse>> GetCurrentStudentQuizDoneAsync();
        Task<List<StudentWorkResult>> GetCurrentStudentQuizDoneWorkAsync(Guid examId, int? noAttempt);
        Task<QuizResultResponse> GradeQuizMCAsync(QuizMCRequest quizStudentWork);
        //Task<QuizResultResponse> GradeQuizFCAsync(Guid classId, Guid examId, double score);
        Task<QuizResultResponse> GradeQuizFCAsync(QuizFCRequest quizFCStudentWork);
        Task<string> GradeExamOffLineAsync(ExamOffLineRequest exaOffLineStudentWork);
        Task<string> EvaluateExamOnLineAsync(Guid studentId, Guid examId, string status, int? noAttempt);
        Task<string> SettingExamTimeAsync(Guid examId, Guid classId, SettingQuizTimeRequest settingInfor);
    }
}
