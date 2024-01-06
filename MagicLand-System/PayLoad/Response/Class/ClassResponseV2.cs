using MagicLand_System.PayLoad.Response.Schedule;
using MagicLand_System.PayLoad.Response.User;

namespace MagicLand_System.PayLoad.Response.Class
{
    public class ClassResponseV2 : ClassResponse
    {
        public string LecturerName { get; set; }
        public List<string>? Schedules { get; set; }
    }
}
