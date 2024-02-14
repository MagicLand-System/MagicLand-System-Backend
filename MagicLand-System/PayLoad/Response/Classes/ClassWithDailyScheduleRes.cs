using MagicLand_System.PayLoad.Response.Schedules;
using MagicLand_System.PayLoad.Response.Users;

namespace MagicLand_System.PayLoad.Response.Classes
{
    public class ClassWithDailyScheduleRes : ClassResponse
    {
        public required UserResponse Lecture { get; set; }
        public List<DailySchedule>? Schedules { get; set; }
    }
}
