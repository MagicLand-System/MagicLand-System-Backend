using MagicLand_System.PayLoad.Response.Schedules;
using MagicLand_System.PayLoad.Response.Slots;

namespace MagicLand_System.PayLoad.Response.Classes
{
    public class ClassResponseForLecture : ClassResponse
    {
        public List<ScheduleWithoutLectureResponse>? ScheduleInfors { get; set; } = new List<ScheduleWithoutLectureResponse>();
    }
}
