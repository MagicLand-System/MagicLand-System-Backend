using MagicLand_System.PayLoad.Request.Quizzes;
using MagicLand_System.PayLoad.Response.Custom;
using MagicLand_System.PayLoad.Response.Quizzes;
using MagicLand_System.PayLoad.Response.Quizzes.Questions;
using MagicLand_System.PayLoad.Response.Quizzes.Result;
using MagicLand_System.PayLoad.Response.Quizzes.Result.Final;
using MagicLand_System.PayLoad.Response.Quizzes.Result.Student;

namespace MagicLand_System.Services.Interfaces
{
    public interface IQuizService
    {
        Task<List<StudenInforAndScore>> GetStudentInforAndScoreAsync(Guid classId, Guid? studentId, Guid? examId);
        Task<List<FinalResultResponse>> GetFinalResultAsync(List<Guid> studentIdList);
        Task<List<QuizResultExtraInforResponse>> GetCurrentStudentQuizDoneAsync();
        Task<List<StudentWorkResult>> GetCurrentStudentQuizDoneWorkAsync(Guid examId, int? noAttempt);
        Task<QuizResultResponse> GradeQuizMCAsync(QuizMCRequest quizStudentWork, TimeOnly doingTime, bool? isCheckingTime);
        Task<QuizResultResponse> GradeQuizFCAsync(QuizFCRequest quizFCStudentWork, TimeOnly doingTime, bool? isCheckingTime);
        Task<string> GradeExamOffLineAsync(ExamOffLineRequest exaOffLineStudentWork, bool? isCheckingTime);
        Task<string> EvaluateExamOnLineAsync(Guid studentId, Guid examId, string status, int? noAttempt);
        Task<string> SettingExamTimeAsync(Guid examId, Guid classId, SettingQuizTimeRequest settingInfor);
        Task<List<ExamWithQuizResponse>> LoadQuizzesAsync();
        Task<List<ExamWithQuizResponse>> LoadQuizzesByCourseIdAsync(Guid id);
        Task<List<ExamResForStudent>> LoadExamOfClassByClassIdAsync(Guid id, Guid? studentId);
        Task<List<ExamExtraClassInfor>> LoadExamOfCurrentStudentAsync(int numberOfDate);
        Task<List<QuizResponse>> GetQuizOfExamtByExamIdAsync(Guid examId, Guid classId, int? examPart, bool isCheckingTime);
    }
}
