using MagicLand_System.PayLoad.Response.Schedule;
using MagicLand_System.PayLoad.Response.User;

namespace MagicLand_System.PayLoad.Response.Class
{
    public class ClassResponseV2 : ClassResponse
    {
        public string LecturerName { get; set; }
        public List<DailySchedule>? Schedules { get; set; }
        public string? CourseName {  get; set; }    
    }
}
