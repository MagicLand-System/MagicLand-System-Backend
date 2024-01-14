using MagicLand_System.PayLoad.Response.Schedules;

namespace MagicLand_System.PayLoad.Response.Classes
{
    public class MyClassResponse : ClassResponse
    {
        public string LecturerName { get; set; }
        public List<DailySchedule>? Schedules { get; set; }
        public string? CourseName { get; set; }
    }
}
