using MagicLand_System.Domain.Models;
using MagicLand_System.PayLoad.Response.Syllabuses;

namespace MagicLand_System.Mappers.Custom
{
    public class SyllabusCustomMapper
    {

        public static SyllabusWithScheduleResponse fromSyllabusAndClassToSyllabusWithSheduleResponse(Syllabus syllabus, Class cls)
        {
            if (syllabus == null || cls == null)
            {
                throw new NullReferenceException();
            }

            var response = new SyllabusWithScheduleResponse
            {
                SyllabusId = syllabus.Id,
                SyllabusName = syllabus.Name,
                Category = syllabus.SyllabusCategory!.Name,
                EffectiveDate = syllabus.EffectiveDate.ToString(),
                StudentTasks = syllabus.StudentTasks,
                ScoringScale = syllabus.ScoringScale,
                TimePerSession = syllabus.TimePerSession,
                SessionsPerCourse = syllabus.Topics!.SelectMany(tp => tp.Sessions!).Count(),
                MinAvgMarkToPass = syllabus.MinAvgMarkToPass,
                Description = syllabus.Description,
                SubjectCode = syllabus.SubjectCode,
                SyllabusLink = syllabus.SyllabusLink,
                SyllabusInformations = SessionCustomMapper.fromTopicsToSyllabusInforResponse(syllabus.Topics),
                Materials = MaterialCustomMapper.fromMaterialsToMaterialResponse(syllabus.Materials!),
                QuestionPackages = QuestionCustomMapper.fromTopicsToQuestionPackageResponse(syllabus.Topics!),
                Exams = ExamSyllabusCustomMapper.fromExamSyllabusesToExamSyllabusResponse(syllabus.ExamSyllabuses!),
                Schedules = ScheduleCustomMapper.fromClassRelatedItemsToScheduleResWithSession(cls.Schedules.ToList(), syllabus.Topics!.ToList()),
            };

            return response;
        }
    }
}