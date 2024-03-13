using MagicLand_System.PayLoad.Request.Quizzes;
using MagicLand_System.PayLoad.Response.Quizzes.Result;

namespace MagicLand_System.Services.Interfaces
{
    public interface IQuizService
    {
        Task<QuizResultResponse> GradeQuizMCAsync(QuizMCRequest quizStudentWork);
        Task<QuizResultResponse> GradeQuizFCAsync(QuizFCRequest quizStudentWork);
    }
}
