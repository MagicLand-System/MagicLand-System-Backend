using MagicLand_System.Domain.Models;
using MagicLand_System.PayLoad.Response.Syllabuses;

namespace MagicLand_System.Mappers.Custom
{
    public class SyllabusCustomMapper
    {
        public static SyllabusResponse fromSyllabusToSyllabusResponse(CourseSyllabus courseSyllabus)
        {
            if (courseSyllabus == null)
            {
                return new SyllabusResponse();
            }

            SyllabusResponse response = new SyllabusResponse()
            {
                SyllabusId = courseSyllabus.Id,
                Name = courseSyllabus.Name ??= "Undefined",
                UpdateTime = courseSyllabus.UpdateTime,
                Topics = courseSyllabus.Topics.Select(tp => TopicCustomMapper.fromTopicToTopicResponse(tp, default)).ToList(),
            };

            return response;
        }
    }
}
