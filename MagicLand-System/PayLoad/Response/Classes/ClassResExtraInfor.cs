using MagicLand_System.PayLoad.Response.Schedules;
using MagicLand_System.PayLoad.Response.Users;

namespace MagicLand_System.PayLoad.Response.Classes
{
    public class ClassResExtraInfor : ClassResponse
    {
        public UserResponse? Lecture { get; set; } = new UserResponse();
        public List<ScheduleResWithTopic>? Schedules { get; set; } = new List<ScheduleResWithTopic>();
    }
}
