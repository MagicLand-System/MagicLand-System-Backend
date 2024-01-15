using MagicLand_System.PayLoad.Response.Rooms;
using MagicLand_System.PayLoad.Response.Schedules;
using MagicLand_System.PayLoad.Response.Users;

namespace MagicLand_System.PayLoad.Response.Classes
{
    public class MyClassResponse : ClassResponse
    {
        public string LecturerName { get; set; }
        public List<DailySchedule>? Schedules { get; set; }
        public string? CourseName { get; set; }
        public RoomResponse? RoomResponse { get; set; }
        public LecturerResponse? LecturerResponse { get; set; }
    }
}
