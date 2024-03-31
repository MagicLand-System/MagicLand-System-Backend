using MagicLand_System.PayLoad.Response.Schedules.ForLecturer;
using MagicLand_System.PayLoad.Response.Sessions;
using MagicLand_System.PayLoad.Response.Topics;

namespace MagicLand_System.PayLoad.Response.Schedules
{
    public class ScheduleResWithSession : ScheduleWithoutLectureResponse
    {
        public SessionResponse? Session { get; set; } = new SessionResponse();
    }
}
