using MagicLand_System.PayLoad.Response.Courses;

namespace MagicLand_System.PayLoad.Response.Syllabuses
{
    public class SyllabusWithCourseResponse : SyllabusResponse
    {
        public CourseSimpleResponse? Course { get; set; } = new CourseSimpleResponse();
    }
}
