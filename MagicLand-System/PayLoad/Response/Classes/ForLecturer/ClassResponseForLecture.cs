using MagicLand_System.PayLoad.Response.Schedules.ForLecturer;

namespace MagicLand_System.PayLoad.Response.Classes.ForLecturer
{
    public class ClassResponseForLecture : ClassResponse
    {
        public List<ScheduleWithoutLectureResponse>? ScheduleInfors { get; set; } = new List<ScheduleWithoutLectureResponse>();
    }
}
