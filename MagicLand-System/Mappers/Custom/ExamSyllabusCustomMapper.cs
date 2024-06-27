using MagicLand_System.Domain.Models;
using MagicLand_System.Helpers;
using MagicLand_System.PayLoad.Response.Slots;
using MagicLand_System.PayLoad.Response.Syllabuses;

namespace MagicLand_System.Mappers.Custom
{
    public class ExamSyllabusCustomMapper
    {
        public static List<ExamSyllabusResponse> fromExamSyllabusesToExamSyllabusResponse(ICollection<ExamSyllabus> exams)
        {
            if (exams == null)
            {
                return default!;
            }

            var responses = exams.Select(exam => new ExamSyllabusResponse
            {
                Type = exam.Category,
                Weight = exam.Weight,
                CompletionCriteria = exam.CompletionCriteria,
                QuestionType = exam.QuestionType != null
                ? string.Join(",", StringHelper.FromStringToList(exam.QuestionType)) : "Participation",
                Part = exam.Part,
            }).ToList();

            return responses;
        }
    }
}
