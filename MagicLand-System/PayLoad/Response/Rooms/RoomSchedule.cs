using MagicLand_System.PayLoad.Response.Users;

namespace MagicLand_System.PayLoad.Response.Rooms
{
    public class RoomSchedule
    {
        public string StartTime { get; set; }
        public string EndTime { get; set; }
        public DateTime Date { get; set; }
        public string ClassCode { get; set; }
        public bool IsUse { get; set; }
        public LecturerResponse LecturerResponse { get; set; }  
    }
}
