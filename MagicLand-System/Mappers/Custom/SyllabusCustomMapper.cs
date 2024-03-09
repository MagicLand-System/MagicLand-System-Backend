using MagicLand_System.Domain.Models;
using MagicLand_System.PayLoad.Response.Syllabuses;

namespace MagicLand_System.Mappers.Custom
{
    public class SyllabusCustomMapper
    {
        public static SyllabusInforResponse RenderSyllabusInforResponse(ICollection<Topic> topics)
        {
            if (topics == null)
            {
                throw new NullReferenceException();
            }

            var response = new SyllabusInforResponse
            {
                Topics = TopicCustomMapper.fromTopicsToTopicResponses(topics),
            };

            return response;
        }

        public static SyllabusResponse fromSyllabusAndClassToSyllabusResponseWithSheduleResponse(Syllabus syllabus, Class cls)
        {
            if (syllabus == null || cls == null)
            {
                throw new NullReferenceException();
            }

            var response = new SyllabusResponse
            {
                Course = CourseCustomMapper.fromCourseToCourseSimpleResponse(syllabus.Course!),
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
                SyllabusInformations = GenerateSyllabusInforResponse(syllabus.Topics!, cls.Schedules),
                Materials = MaterialCustomMapper.fromMaterialsToMaterialResponse(syllabus.Materials!),
                QuestionPackages = QuestionCustomMapper.fromTopicsToQuestionPackageResponse(syllabus.Topics!),
                Exams = ExamSyllabusCustomMapper.fromExamSyllabusesToExamSyllabusResponse(syllabus.ExamSyllabuses!),
            };

            return response;
        }

        private static SyllabusInforResponse GenerateSyllabusInforResponse(ICollection<Topic> topics, ICollection<Schedule> schedules)
        {
            if (topics == null || schedules == null)
            {
                return default!;
            }

            var response = new SyllabusInforResponse
            {
                Topics = TopicCustomMapper.fromTopicsAndScheduleToTopicResponses(topics, schedules),
            };

            return response;
        }
    }
}