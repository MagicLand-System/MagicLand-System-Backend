using MagicLand_System.PayLoad.Response.Schedule;
using MagicLand_System.PayLoad.Response.User;

namespace MagicLand_System.PayLoad.Response.Class
{
    public class ClassResponseV1 : ClassResponse
    {
        public UserResponse? Lecture { get; set; }
        public List<ScheduleResponse>? Schedules { get; set; }
    }
}
