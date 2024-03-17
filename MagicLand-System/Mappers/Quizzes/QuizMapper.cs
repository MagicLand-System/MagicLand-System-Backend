using AutoMapper;
using MagicLand_System.Domain.Models;
using MagicLand_System.PayLoad.Response.Quizes;
using MagicLand_System.PayLoad.Response.Quizzes;

namespace MagicLand_System.Mappers.Quizzes
{
    public class QuizMapper : Profile
    {
        public QuizMapper()
        {
            CreateMap<ExamResponse, ExamWithScore>();
        }

    }
}
