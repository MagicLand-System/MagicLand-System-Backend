using MagicLand_System.PayLoad.Response.Schedules;
using MagicLand_System.PayLoad.Response.Users;

namespace MagicLand_System.PayLoad.Response.Classes
{
    public class ClassResExtraInfor : ClassResponse
    {
        public string? CourseName { get; set; }
        public UserResponse? Lecture { get; set; } = new UserResponse();
        public List<ScheduleResWithSession>? Schedules { get; set; } = new List<ScheduleResWithSession>();
    }
}
