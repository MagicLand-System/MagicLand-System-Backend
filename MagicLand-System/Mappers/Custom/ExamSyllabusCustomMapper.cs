using MagicLand_System.Domain.Models;
using MagicLand_System.PayLoad.Response.Slots;
using MagicLand_System.PayLoad.Response.Syllabuses;

namespace MagicLand_System.Mappers.Custom
{
    public class ExamSyllabusCustomMapper
    {
        public static ExamSyllabusResponse fromExamSyllabusesToExamSyllabusResponse(ICollection<ExamSyllabus> exams)
        {
            if (exams == null)
            {
                return default!;
            }

            var response = new ExamSyllabusResponse
            {
                ExamSyllabusInfor = exams.Select(exam => new ExamSyllabusInforResponse
                {
                    Type = exam.Category,
                    Weight = exam.Weight,
                    CompletionCriteria = exam.CompleteionCriteria,
                    Duration = exam.Duration,
                    QuestionType = exam.QuestionType,
                    Part = exam.Part,
                }).ToList(),
            };

            return response;
        }
    }
}
